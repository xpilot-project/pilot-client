/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2020 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Protobuf;

namespace Vatsim.Xpilot.Simulator
{
    public class XplaneAdapter : EventBus, IXplaneAdapter
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.UserAircraftDataUpdated)]
        public event EventHandler<UserAircraftDataUpdatedEventArgs> UserAircraftDataUpdated;

        [EventPublication(EventTopics.UserAircraftConfigDataUpdated)]
        public event EventHandler<UserAircraftConfigDataUpdatedEventArgs> UserAircraftConfigDataUpdated;

        [EventPublication(EventTopics.SimulatorConnected)]
        public event EventHandler<EventArgs> SimulatorConnected;

        [EventPublication(EventTopics.SimulatorDisconnected)]
        public event EventHandler<EventArgs> SimulatorDisconnected;

        [EventPublication(EventTopics.RadioStackStateChanged)]
        public event EventHandler<RadioStackStateChangedEventArgs> RadioStackStateChanged;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotifictionSoundEventArgs> PlayNotificationSound;

        [EventPublication(EventTopics.PushToTalkStateChanged)]
        public event EventHandler<PushToTalkStateChangedEventArgs> PushToTalkStateChanged;

        [EventPublication(EventTopics.RadioVolumeChanged)]
        public event EventHandler<RadioVolumeChangedEventArgs> RadioVolumeChanged;

        [EventPublication(EventTopics.ConnectButtonEnabled)]
        public event EventHandler<EventArgs> ConnectButtonEnabled;

        [EventPublication(EventTopics.ConnectButtonDisabled)]
        public event EventHandler<EventArgs> ConnectButtonDisabled;

        private readonly INetworkManager mFsdManager;
        private readonly IAppConfig mConfig;

        private readonly NetMQPoller mPoller;
        private readonly NetMQQueue<byte[]> mMessageQueue;
        private readonly DealerSocket mDealerSocket;
        private readonly List<DealerSocket> mVisualDealerSockets;

        private readonly Timer mConnectionTimer;
        private readonly Timer mNearbyControllersRefresh;

        private readonly List<DateTime> mConnectionHeartbeats = new List<DateTime>();
        private readonly List<Controller> mControllers = new List<Controller>();

        private UserAircraftData mUserAircraftData;
        private UserAircraftConfigData mUserAircraftConfigData;
        private RadioStackState mRadioStackState;

        private bool mInvalidPluginVersionShown = false;
        private bool mReceivedInitialHandshake = false;
        private bool mValidCsl = false;
        private string mSimulatorIP = "127.0.0.1";

        public XplaneAdapter(IEventBroker broker, IAppConfig config, INetworkManager fsdManager) : base(broker)
        {
            DealerSocket visualDealerSocket = null;
            mVisualDealerSockets = null;
            mConfig = config;
            mFsdManager = fsdManager;

            if (mConfig.VisualClientIPs.Count > 0)
            {
                foreach (string mIP in mConfig.VisualClientIPs)
                {
                    visualDealerSocket = new DealerSocket();
                    visualDealerSocket.Options.Identity = Encoding.UTF8.GetBytes("CLIENT");
                    visualDealerSocket.Options.TcpKeepalive = true;
                    try
                    {
                        visualDealerSocket.Connect("tcp://" + mIP + ":" + mConfig.SimulatorPort);
                        if (mVisualDealerSockets == null) mVisualDealerSockets = new List<DealerSocket>();
                        mVisualDealerSockets.Add(visualDealerSocket);
                    }
                    catch (AddressAlreadyInUseException)
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Plugin port already in use."));
                    }
                }
            }

            if (!string.IsNullOrEmpty(mConfig.SimulatorIP))
            {
                mSimulatorIP = mConfig.SimulatorIP;
            }

            mMessageQueue = new NetMQQueue<byte[]>();
            mDealerSocket = new DealerSocket();
            mDealerSocket.Options.TcpKeepalive = true;
            mDealerSocket.Options.Identity = Encoding.UTF8.GetBytes("CLIENT");
            mDealerSocket.ReceiveReady += DealerSocket_ReceiveReady;
            try
            {
                mDealerSocket.Connect("tcp://" + mSimulatorIP + ":" + mConfig.SimulatorPort);
            }
            catch (AddressAlreadyInUseException)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Plugin port already in use."));
            }
            using (mPoller = new NetMQPoller { mDealerSocket, mMessageQueue })
            {
                if (!mPoller.IsRunning)
                {
                    mPoller.RunAsync();
                }
            }

            mMessageQueue.ReceiveReady += (s, e) =>
            {
                if (mMessageQueue.TryDequeue(out byte[] data, TimeSpan.FromMilliseconds(100)))
                {
                    if (mDealerSocket != null)
                    {
                        mDealerSocket.SendFrame(data);
                    }
                    if (mVisualDealerSockets != null && mVisualDealerSockets.Count > 0)
                    {
                        foreach (DealerSocket socket in mVisualDealerSockets)
                        {
                            socket.SendFrame(data);
                        }
                    }
                }
            };

            mUserAircraftData = new UserAircraftData();
            mUserAircraftConfigData = new UserAircraftConfigData();
            mRadioStackState = new RadioStackState();

            mConnectionTimer = new Timer
            {
                Interval = 1000
            };
            mConnectionTimer.Tick += ConnectionTimer_Tick;
            mConnectionTimer.Start();
        }

        [EventSubscription(EventTopics.RadioVolumeChanged, typeof(OnUserInterfaceAsync))]
        public void OnVolumeLevelsChanged(object sender, RadioVolumeChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 1:
                    //mXplaneConnector.SetDataRefValue(new DataRefElement
                    //{
                    //    DataRef = "sim/cockpit2/radios/actuators/audio_volume_com1"
                    //}, e.Volume);
                    break;
                case 2:
                    //mXplaneConnector.SetDataRefValue(new DataRefElement
                    //{
                    //    DataRef = "sim/cockpit2/radios/actuators/audio_volume_com2"
                    //}, e.Volume);
                    break;
            }
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChangedEventArgs e)
        {
            //if (mFsdManager.IsConnected)
            //{
            //    var ptt = new DataRefElement
            //    {
            //        DataRef = "xpilot/ptt"
            //    };
            //    mXplaneConnector.SetDataRefValue(ptt, e.Down ? 1.0f : 0.0f);
            //}
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mNearbyControllersRefresh.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            ClearNearbyControllersMessage();
            NetworkDisconnected();
            RemoveAllPlanes();
            mNearbyControllersRefresh.Stop();
            mControllers.Clear();
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From != "SERVER")
            {
                var msg = new Wrapper
                {
                    PrivateMessageReceived = new Protobuf.PrivateMessageReceived()
                };
                msg.PrivateMessageReceived.From = e.From.ToUpper();
                msg.PrivateMessageReceived.Message = e.Data;
                SendProtobufArray(msg.ToByteArray());
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            var msg = new Wrapper
            {
                PrivateMessageSent = new Protobuf.PrivateMessageSent()
            };
            msg.PrivateMessageSent.To = e.To.ToUpper();
            msg.PrivateMessageSent.Message = e.Message;
            SendProtobufArray(msg.ToByteArray());
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            RequestPluginVersion();
            RequestPluginHash();
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterfaceAsync))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            mPoller.Stop();
            if (mDealerSocket != null)
            {
                mDealerSocket.Close();
            }
            if (mVisualDealerSockets != null && mVisualDealerSockets.Count > 0)
            {
                foreach (DealerSocket socket in mVisualDealerSockets)
                {
                    socket.Close();
                }

            }
        }

        [EventSubscription(EventTopics.ValidateCslPaths, typeof(OnUserInterfaceAsync))]
        public void OnValidateCslPaths(object sender, EventArgs e)
        {
            RequestCslValidation();
        }

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            byte[] bytes = e.Socket.ReceiveFrameBytes();
            if (bytes.Length > 0)
            {
                var wrapper = Wrapper.Parser.ParseFrom(bytes);

                switch (wrapper.MsgCase)
                {
                    case Wrapper.MsgOneofCase.PluginHash:
                        {
                            if (wrapper.PluginHash.HasHash)
                            {
                                mFsdManager.SetPluginHash(wrapper.PluginHash.Hash);
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.PluginVersion:
                        {
                            if (wrapper.PluginVersion.HasVersion)
                            {
                                mReceivedInitialHandshake = true;
                                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                                string compactedVersion = string.Format($"{v.Major}{v.Minor}{v.Build}");
                                int versionInt = int.Parse(compactedVersion);
                                if (wrapper.PluginVersion.Version != versionInt)
                                {
                                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
                                    if (!mInvalidPluginVersionShown)
                                    {
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot."));
                                        PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                        mInvalidPluginVersionShown = true;
                                    }
                                }
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.CslValidate:
                        {
                            if (wrapper.CslValidate.HasValid)
                            {
                                mValidCsl = wrapper.CslValidate.Valid;
                                if (!wrapper.CslValidate.Valid)
                                {
                                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
                                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Settings). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models."));
                                    PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                }
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.XplaneDatarefs:
                        {
                            if (wrapper.XplaneDatarefs.HasAudioComSelection)
                            {
                                mRadioStackState.Com1TransmitEnabled = (uint)wrapper.XplaneDatarefs.AudioComSelection == 6;
                                mRadioStackState.Com2TransmitEnabled = (uint)wrapper.XplaneDatarefs.AudioComSelection == 7;
                            }

                            // com1
                            if (wrapper.XplaneDatarefs.HasCom1Freq)
                            {
                                mRadioStackState.Com1ActiveFrequency = ((uint)wrapper.XplaneDatarefs.Com1Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneDatarefs.HasCom1AudioSelection)
                            {
                                mRadioStackState.Com1ReceiveEnabled = wrapper.XplaneDatarefs.Com1AudioSelection;
                            }
                            if (wrapper.XplaneDatarefs.HasCom1Volume)
                            {
                                float val = wrapper.XplaneDatarefs.Com1Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(1, val));
                            }

                            // com2
                            if (wrapper.XplaneDatarefs.HasCom2Freq)
                            {
                                mRadioStackState.Com2ActiveFrequency = ((uint)wrapper.XplaneDatarefs.Com2Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneDatarefs.HasCom2AudioSelection)
                            {
                                mRadioStackState.Com2ReceiveEnabled = wrapper.XplaneDatarefs.Com2AudioSelection;
                            }
                            if (wrapper.XplaneDatarefs.HasCom2Volume)
                            {
                                float val = wrapper.XplaneDatarefs.Com2Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(2, val));
                            }

                            if (wrapper.XplaneDatarefs.HasAvionicsPowerOn)
                            {
                                mRadioStackState.AvionicsPowerOn = wrapper.XplaneDatarefs.AvionicsPowerOn;
                            }

                            if (wrapper.XplaneDatarefs.HasLatitude)
                            {
                                mUserAircraftData.Latitude = wrapper.XplaneDatarefs.Latitude;
                            }
                            if (wrapper.XplaneDatarefs.HasLongitude)
                            {
                                mUserAircraftData.Longitude = wrapper.XplaneDatarefs.Longitude;
                            }
                            if (wrapper.XplaneDatarefs.HasAltitude)
                            {
                                mUserAircraftData.AltitudeTrue = wrapper.XplaneDatarefs.Altitude;
                            }
                            if (wrapper.XplaneDatarefs.HasPressureAltitude)
                            {
                                mUserAircraftData.AltitudePressure = wrapper.XplaneDatarefs.PressureAltitude;
                            }
                            if (wrapper.XplaneDatarefs.HasYaw)
                            {
                                mUserAircraftData.Heading = wrapper.XplaneDatarefs.Yaw;
                            }
                            if (wrapper.XplaneDatarefs.HasPitch)
                            {
                                mUserAircraftData.Pitch = wrapper.XplaneDatarefs.Pitch;
                            }
                            if (wrapper.XplaneDatarefs.HasRoll)
                            {
                                mUserAircraftData.Bank = wrapper.XplaneDatarefs.Roll;
                            }
                            if (wrapper.XplaneDatarefs.HasGroundSpeed)
                            {
                                mUserAircraftData.SpeedGround = wrapper.XplaneDatarefs.GroundSpeed;
                            }
                            if (wrapper.XplaneDatarefs.HasTransponderCode)
                            {
                                mRadioStackState.TransponderCode = (ushort)wrapper.XplaneDatarefs.TransponderCode;
                            }
                            if (wrapper.XplaneDatarefs.HasTransponderMode)
                            {
                                mRadioStackState.SquawkingModeC = wrapper.XplaneDatarefs.TransponderMode >= 2;
                            }
                            if (wrapper.XplaneDatarefs.HasTransponderIdent)
                            {
                                mRadioStackState.SquawkingIdent = wrapper.XplaneDatarefs.TransponderIdent;
                                //SquawkingIdentChanged?.Invoke(this, new TransponderIdentStateChanged(wrapper.XplaneDatarefs.TransponderIdent));
                            }

                            // lights
                            if (wrapper.XplaneDatarefs.HasBeaconLightsOn)
                            {
                                mUserAircraftConfigData.BeaconOn = wrapper.XplaneDatarefs.BeaconLightsOn;
                            }
                            if (wrapper.XplaneDatarefs.HasLandingLightsOn)
                            {
                                mUserAircraftConfigData.LandingLightsOn = wrapper.XplaneDatarefs.LandingLightsOn;
                            }
                            if (wrapper.XplaneDatarefs.HasNavLightsOn)
                            {
                                mUserAircraftConfigData.NavLightOn = wrapper.XplaneDatarefs.NavLightsOn;
                            }
                            if (wrapper.XplaneDatarefs.HasStrobeLightsOn)
                            {
                                mUserAircraftConfigData.StrobesOn = wrapper.XplaneDatarefs.StrobeLightsOn;
                            }
                            if (wrapper.XplaneDatarefs.HasTaxiLightsOn)
                            {
                                mUserAircraftConfigData.TaxiLightsOn = wrapper.XplaneDatarefs.TaxiLightsOn;
                            }

                            // controls
                            if (wrapper.XplaneDatarefs.HasFlaps)
                            {
                                mUserAircraftConfigData.FlapsRatio = wrapper.XplaneDatarefs.Flaps;
                            }
                            if (wrapper.XplaneDatarefs.HasSpeedBrakes)
                            {
                                mUserAircraftConfigData.SpoilersRatio = wrapper.XplaneDatarefs.SpeedBrakes;
                            }
                            if (wrapper.XplaneDatarefs.HasGearDown)
                            {
                                mUserAircraftConfigData.GearDown = wrapper.XplaneDatarefs.GearDown;
                            }
                            if (wrapper.XplaneDatarefs.HasOnGround)
                            {
                                mUserAircraftConfigData.OnGround = wrapper.XplaneDatarefs.OnGround;
                            }

                            if (wrapper.XplaneDatarefs.HasEngineCount)
                            {
                                mUserAircraftConfigData.EngineCount = wrapper.XplaneDatarefs.EngineCount;
                            }
                            if (wrapper.XplaneDatarefs.HasEngine1Running)
                            {
                                mUserAircraftConfigData.Engine1Running = wrapper.XplaneDatarefs.Engine1Running;
                            }
                            if (wrapper.XplaneDatarefs.HasEngine2Running)
                            {
                                mUserAircraftConfigData.Engine2Running = wrapper.XplaneDatarefs.Engine2Running;
                            }
                            if (wrapper.XplaneDatarefs.HasEngine3Running)
                            {
                                mUserAircraftConfigData.Engine3Running = wrapper.XplaneDatarefs.Engine3Running;
                            }
                            if (wrapper.XplaneDatarefs.HasEngine4Running)
                            {
                                mUserAircraftConfigData.Engine4Running = wrapper.XplaneDatarefs.Engine4Running;
                            }

                            mConnectionHeartbeats.Add(wrapper.Timestamp.ToDateTime());
                            UserAircraftDataUpdated?.Invoke(this, new UserAircraftDataUpdatedEventArgs(mUserAircraftData));
                            RadioStackStateChanged?.Invoke(this, new RadioStackStateChangedEventArgs(mRadioStackState));
                        }
                        break;
                }
            }

            //if (!string.IsNullOrEmpty(command))
            //{
            //    try
            //    {
            //        var json = JsonConvert.DeserializeObject<XplaneConnect>(command);
            //        dynamic data = json.Data;
            //        switch (json.Type)
            //        {
            //            case XplaneConnect.MessageType.PluginHash:
            //                string hash = data.Hash;
            //                if (!string.IsNullOrEmpty(hash))
            //                {
            //                    mFsdManager.ClientProperties.PluginHash = hash;
            //                }
            //                break;
            //            case XplaneConnect.MessageType.RequestAtis:
            //                string callsign = data.Callsign;
            //                if (!string.IsNullOrEmpty(callsign))
            //                {
            //                    RequestControllerAtisSent?.Invoke(this, new RequestControllerAtisEventArgs(callsign));
            //                }
            //                break;
            //            case XplaneConnect.MessageType.PluginVersion:
            //                mReceivedInitialHandshake = true;
            //                int pluginVersion = (int)data.Version;
            //                if (pluginVersion != MIN_PLUGIN_VERSION)
            //                {
            //                    EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
            //                    if (!mInvalidPluginVersionShown)
            //                    {
            //                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot."));
            //                        PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            //                        mInvalidPluginVersionShown = true;
            //                    }
            //                }
            //                break;
            //            case XplaneConnect.MessageType.SocketMessage:
            //                {
            //                    string msg = data.Message;
            //                    if (!string.IsNullOrEmpty(msg))
            //                    {
            //                        RaiseSocketMessageReceived?.Invoke(this, new ClientEventArgs<string>(msg));
            //                    }
            //                }
            //                break;
            //            case XplaneConnect.MessageType.PrivateMessageSent:
            //                {
            //                    string msg = data.Message;
            //                    string to = data.To;
            //                    if (mFsdManager.IsConnected)
            //                    {
            //                        PrivateMessageSent?.Invoke(this, new PrivateMessageSentEventArgs(mFsdManager.OurCallsign, to, msg));
            //                    }
            //                }
            //                break;
            //            case XplaneConnect.MessageType.ValidateCslPaths:
            //                mValidCsl = (bool)data.Result;
            //                if (data.Result == false)
            //                {
            //                    EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
            //                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Preferences). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models."));
            //                    PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            //                }
            //                break;
            //            case XplaneConnect.MessageType.ForceDisconnect:
            //                string reason = data.Reason;
            //                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, reason));
            //                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            //                mFsdManager.Disconnect(new DisconnectInfo
            //                {
            //                    Type = DisconnectType.Intentional
            //                });
            //                break;
            //        }
            //    }
            //    catch (JsonException ex)
            //    {
            //        //NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error deserializing JSON object: " + ex.Message));
            //    }
            //}
        }

        private void RequestPluginHash()
        {
            var msg = new Wrapper
            {
                PluginHash = new PluginHash()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        private void RequestPluginVersion()
        {
            var msg = new Wrapper
            {
                PluginVersion = new PluginVersion()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        private void RequestCslValidation()
        {
            var msg = new Wrapper
            {
                CslValidate = new CslValidate()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        private void SendProtobufArray(byte[] data)
        {
            if (data.Length > 0)
            {
                mMessageQueue.Enqueue(data);
            }
        }

        //private void NearbyControllersRefresh_Tick(object sender, EventArgs e)
        //{
        //    List<NearbyControllers.Types.Controller> temp = new List<NearbyControllers.Types.Controller>();
        //    foreach (var controller in mControllers)
        //    {
        //        temp.Add(new NearbyControllers.Types.Controller
        //        {
        //            Callsign = controller.Callsign,
        //            Frequency = (controller.NormalizedFrequency.FsdFrequencyToHertz() / 1000000.0).ToString("0.000"),
        //            XplaneFrequency = (int)controller.NormalizedFrequency + 100000,
        //            RealName = controller.RealName
        //        });
        //    }
        //    var msg = new Wrapper
        //    {
        //        NearbyControllers = new Xpilot.NearbyControllers()
        //    };
        //    msg.NearbyControllers.List.AddRange(temp.OrderBy(a => a.Callsign));
        //    SendProtobufArray(msg.ToByteArray());
        //}

        private void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (mConnectionHeartbeats.Count > 0)
            {
                TimeSpan span = DateTime.UtcNow - mConnectionHeartbeats.Last();
                if (span.TotalSeconds < 15)
                {
                    if (!mReceivedInitialHandshake)
                    {
                        SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                        ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);

                        RequestPluginVersion();
                        RequestPluginHash();
                    }
                    else
                    {
                        SimulatorConnected?.Invoke(this, EventArgs.Empty);
                        if (mValidCsl && !mInvalidPluginVersionShown)
                        {
                            ConnectButtonEnabled?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    mConnectionHeartbeats.RemoveAt(0);
                }
                else
                {
                    mReceivedInitialHandshake = false;
                    SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                mReceivedInitialHandshake = false;
                SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetTransponderCode(int code)
        {
            var msg = new Wrapper
            {
                SetTransponderCode = new SetTransponderCode()
            };
            msg.SetTransponderCode.Code = code;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SetRadioFrequency(int radio, uint freq)
        {

        }

        public void SetAudioComSelection(int radio)
        {

        }

        public void SetAudioSelectionCom1(bool enabled)
        {

        }

        public void SetAudioSelectionCom2(bool enabled)
        {

        }

        public void SetLoginStatus(bool connected)
        {

        }

        public void AddPlane(NetworkAircraft plane)
        {
            var msg = new Wrapper
            {
                AddPlane = new AddPlane()
            };
            msg.AddPlane.Callsign = plane.Callsign;
            msg.AddPlane.Equipment = plane.Equipment;
            msg.AddPlane.Airline = plane.Airline;
            SendProtobufArray(msg.ToByteArray());
        }

        public void PlanePoseChanged(NetworkAircraft plane, NetworkAircraftState pose)
        {
            var msg = new Wrapper
            {
                PositionUpdate = new PositionUpdate()
            };
            msg.PositionUpdate.Callsign = plane.Callsign;
            msg.PositionUpdate.Latitude = pose.Location.Lat;
            msg.PositionUpdate.Longitude = pose.Location.Lon;
            msg.PositionUpdate.Altitude = pose.Altitude;
            msg.PositionUpdate.Bank = pose.Bank;
            msg.PositionUpdate.Pitch = pose.Pitch;
            msg.PositionUpdate.Heading = pose.Heading;
            //msg.PositionUpdate.TransponderCode = pose.Transponder.TransponderCode;
            //msg.PositionUpdate.TransponderModeC = pose.Transponder.TransponderModeC;
            SendProtobufArray(msg.ToByteArray());
        }

        public void PlaneConfigChanged(AirplaneConfig config)
        {
            var msg = new Wrapper
            {
                AirplaneConfig = config
            };
            SendProtobufArray(msg.ToByteArray());
        }

        public void RemovePlane(NetworkAircraft plane)
        {
            var msg = new Wrapper
            {
                RemovePlane = new RemovePlane()
            };
            msg.RemovePlane.Callsign = plane.Callsign;
            SendProtobufArray(msg.ToByteArray());
        }

        public void RemoveAllPlanes()
        {
            var msg = new Wrapper
            {
                RemoveAllPlanes = new RemoveAllPlanes()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        public void NetworkConnected(string callsign)
        {
            var msg = new Wrapper
            {
                NetworkConnected = new Protobuf.NetworkConnected()
            };
            msg.NetworkConnected.Callsign = callsign;
            SendProtobufArray(msg.ToByteArray());
        }

        public void NetworkDisconnected()
        {
            var msg = new Wrapper
            {
                NetworkDisconnected = new Protobuf.NetworkDisconnected()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        private void ClearNearbyControllersMessage()
        {
            var msg = new Wrapper
            {
                ClearNearbyControllers = new Protobuf.ClearNearbyControllers()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        public void PlaneConfigChanged(AircraftConfiguration config)
        {
            throw new NotImplementedException();
        }
    }
}