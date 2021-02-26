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
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Controllers;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using NetMQ;
using NetMQ.Sockets;
using Xpilot;
using Google.Protobuf;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Aircraft;

namespace XPilot.PilotClient.XplaneAdapter
{
    public class XplaneConnectionManager : EventBus, IXplaneConnectionManager
    {
        const int MIN_PLUGIN_VERSION = 1338; // 1.3.10 = 1310

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPosted> RaiseNotificationPosted;

        [EventPublication(EventTopics.UserAircraftDataChanged)]
        public event EventHandler<UserAircraftDataChanged> RaiseUserAircraftDataUpdated;

        [EventPublication(EventTopics.SimConnectionStateChanged)]
        public event EventHandler<ClientEventArgs<bool>> RaiseSimConnectionStateChanged;

        [EventPublication(EventTopics.RadioStackStateChanged)]
        public event EventHandler<RadioStackStateChanged> RaiseRadioStackStateChanged;

        [EventPublication(EventTopics.TransponderModeChanged)]
        public event EventHandler<ClientEventArgs<bool>> RaiseTransponderModeChanged;

        [EventPublication(EventTopics.TransponderIdentStateChanged)]
        public event EventHandler<TransponderIdentStateChanged> SquawkingIdentChanged;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotificationSound> RaisePlaySoundRequested;

        [EventPublication(EventTopics.PushToTalkStateChanged)]
        public event EventHandler<PushToTalkStateChanged> RaisePushToTalkStateChanged;

        [EventPublication(EventTopics.AcarsRequestSent)]
        public event EventHandler<AcarsResponseReceived> RaiseRequestControllerAtisSent;

        [EventPublication(EventTopics.SocketMessageReceived)]
        public event EventHandler<ClientEventArgs<string>> RaiseSocketMessageReceived;

        [EventPublication(EventTopics.SimulatorMessageSent)]
        public event EventHandler<SimulatorMessageSent> RaiseSimulatorMessageSent;

        [EventPublication(EventTopics.ConnectButtonStateChanged)]
        public event EventHandler<ClientEventArgs<bool>> RaiseConnectButtonStateChanged;

        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSent> RaisePrivateMessageSent;

        [EventPublication(EventTopics.RadioVolumeChanged)]
        public event EventHandler<RadioVolumeChanged> RaiseRadioVolumeChanged;

        private readonly IFsdManager mFsdManager;
        private readonly IAppConfig mConfig;

        private readonly NetMQPoller mPoller;
        private readonly NetMQQueue<byte[]> mMessageQueue;
        private readonly DealerSocket mDealerSocket;
        private readonly List<DealerSocket> mVisualDealerSockets;

        private readonly Timer mConnectionTimer;
        private readonly Timer mDataRefreshTimer;
        private readonly Timer mWhosOnlineListRefresh;

        private readonly List<DateTime> mConnectionHeartbeats = new List<DateTime>();
        private readonly List<Controller> mControllers = new List<Controller>();

        private readonly UserAircraftData mUserAircraftData;
        private readonly UserAircraftRadioStack mRadioStackState;

        private bool mInvalidPluginVersionShown = false;
        private bool mReceivedInitialHandshake = false;
        private bool mDisconnectMessageSent = false;
        private bool mValidCsl = false;
        private string mSimulatorIP = "127.0.0.1";

        public XplaneConnectionManager(IEventBroker broker, IAppConfig config, IFsdManager fsdManager) : base(broker)
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
                        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Plugin port already in use. Please choose a different TCP port."));
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
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Plugin port already in use. Please choose a different TCP port."));
            }
            mPoller = new NetMQPoller { mDealerSocket, mMessageQueue };
            if (!mPoller.IsRunning)
            {
                mPoller.RunAsync();
            }

            mMessageQueue.ReceiveReady += (s, e) =>
            {
                if (mMessageQueue.TryDequeue(out byte[] data, TimeSpan.FromMilliseconds(100)))
                {
                    if (mDealerSocket != null)
                    {
                        mDealerSocket.SendFrame(data);
                    }
                    //if (mVisualDealerSockets != null && mVisualDealerSockets.Count > 0)
                    //{
                    //    foreach (DealerSocket socket in mVisualDealerSockets)
                    //    {
                    //        socket.SendFrame(data);
                    //    }
                    //}
                }
            };

            mUserAircraftData = new UserAircraftData();
            mRadioStackState = new UserAircraftRadioStack();

            mDataRefreshTimer = new Timer
            {
                Interval = 20
            };
            mDataRefreshTimer.Tick += DataRefreshTimer_Tick;
            mDataRefreshTimer.Start();

            mConnectionTimer = new Timer
            {
                Interval = 50
            };
            mConnectionTimer.Tick += ConnectionTimer_Tick;
            mConnectionTimer.Start();

            mWhosOnlineListRefresh = new Timer
            {
                Interval = 5000
            };
            mWhosOnlineListRefresh.Tick += WhosOnlineListRefresh_Tick;
        }

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            byte[] bytes = e.Socket.ReceiveFrameBytes();
            if (bytes.Length > 0)
            {
                var wrapper = Wrapper.Parser.ParseFrom(bytes);

                switch(wrapper.MsgCase)
                {
                    case Wrapper.MsgOneofCase.PluginHash:
                        {
                            if (wrapper.PluginHash.HasHash)
                            {
                                mFsdManager.ClientProperties.PluginHash = wrapper.PluginHash.Hash;
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.PluginVersion:
                        {
                            if(wrapper.PluginVersion.HasVersion)
                            {
                                mReceivedInitialHandshake = true;
                                if(wrapper.PluginVersion.Version != MIN_PLUGIN_VERSION)
                                {
                                    RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
                                    if (!mInvalidPluginVersionShown)
                                    {
                                        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot."));
                                        RaisePlaySoundRequested?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
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
                                    RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
                                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Preferences). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models."));
                                    RaisePlaySoundRequested?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
                                }
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.XplaneData:
                        {
                            if (wrapper.XplaneData.HasAudioComSelection)
                            {
                                mRadioStackState.ComAudioSelection = (uint)wrapper.XplaneData.AudioComSelection;
                            }

                            // com1
                            if (wrapper.XplaneData.HasCom1Freq)
                            {
                                mRadioStackState.Com1ActiveFreq = ((uint)wrapper.XplaneData.Com1Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneData.HasCom1StbyFreq)
                            {
                                mRadioStackState.Com1StandbyFreq = ((uint)wrapper.XplaneData.Com1StbyFreq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneData.HasCom1Power)
                            {
                                mRadioStackState.Com1Power = wrapper.XplaneData.Com1Power;
                            }
                            if (wrapper.XplaneData.HasCom1AudioSelection)
                            {
                                mRadioStackState.Com1Listening = wrapper.XplaneData.Com1AudioSelection;
                            }
                            if (wrapper.XplaneData.HasCom1Volume)
                            {
                                float val = wrapper.XplaneData.Com1Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RaiseRadioVolumeChanged?.Invoke(this, new RadioVolumeChanged(1, val));
                            }

                            // com2
                            if (wrapper.XplaneData.HasCom2Freq)
                            {
                                mRadioStackState.Com2ActiveFreq = ((uint)wrapper.XplaneData.Com2Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneData.HasCom2StbyFreq)
                            {
                                mRadioStackState.Com2StandbyFreq = ((uint)wrapper.XplaneData.Com2StbyFreq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.XplaneData.HasCom2Power)
                            {
                                mRadioStackState.Com2Power = wrapper.XplaneData.Com2Power;
                            }
                            if (wrapper.XplaneData.HasCom2AudioSelection)
                            {
                                mRadioStackState.Com2Listening = wrapper.XplaneData.Com2AudioSelection;
                            }
                            if (wrapper.XplaneData.HasCom2Volume)
                            {
                                float val = wrapper.XplaneData.Com2Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RaiseRadioVolumeChanged?.Invoke(this, new RadioVolumeChanged(2, val));
                            }

                            if (wrapper.XplaneData.HasAvionicsPowerOn)
                            {
                                mRadioStackState.AvionicsPower = wrapper.XplaneData.AvionicsPowerOn;
                            }

                            if (wrapper.XplaneData.HasLatitude)
                            {
                                mUserAircraftData.Latitude = wrapper.XplaneData.Latitude;
                            }
                            if (wrapper.XplaneData.HasLongitude)
                            {
                                mUserAircraftData.Longitude = wrapper.XplaneData.Longitude;
                            }
                            if (wrapper.XplaneData.HasAltitude)
                            {
                                mUserAircraftData.AltitudeMsl = wrapper.XplaneData.Altitude;
                            }
                            if (wrapper.XplaneData.HasPressureAltitude)
                            {
                                mUserAircraftData.AltitudeAgl = wrapper.XplaneData.PressureAltitude;
                            }
                            if (wrapper.XplaneData.HasYaw)
                            {
                                mUserAircraftData.Yaw = wrapper.XplaneData.Yaw;
                            }
                            if (wrapper.XplaneData.HasPitch)
                            {
                                mUserAircraftData.Pitch = wrapper.XplaneData.Pitch;
                            }
                            if (wrapper.XplaneData.HasRoll)
                            {
                                mUserAircraftData.Roll = wrapper.XplaneData.Roll;
                            }
                            if (wrapper.XplaneData.HasGroundSpeed)
                            {
                                mUserAircraftData.GroundSpeed = wrapper.XplaneData.GroundSpeed;
                            }
                            if (wrapper.XplaneData.HasTransponderCode)
                            {
                                mUserAircraftData.TransponderCode = wrapper.XplaneData.TransponderCode;
                            }
                            if (wrapper.XplaneData.HasTransponderMode)
                            {
                                mUserAircraftData.TransponderMode = wrapper.XplaneData.TransponderMode;
                            }
                            if (wrapper.XplaneData.HasTransponderIdent)
                            {
                                mUserAircraftData.TransponderIdent = wrapper.XplaneData.TransponderIdent;
                                SquawkingIdentChanged?.Invoke(this, new TransponderIdentStateChanged(wrapper.XplaneData.TransponderIdent));
                            }

                            // lights
                            if (wrapper.XplaneData.HasBeaconLightsOn)
                            {
                                mUserAircraftData.BeaconLightsOn = wrapper.XplaneData.BeaconLightsOn;
                            }
                            if (wrapper.XplaneData.HasLandingLightsOn)
                            {
                                mUserAircraftData.LandingLightsOn = wrapper.XplaneData.LandingLightsOn;
                            }
                            if (wrapper.XplaneData.HasNavLightsOn)
                            {
                                mUserAircraftData.NavLightsOn = wrapper.XplaneData.NavLightsOn;
                            }
                            if (wrapper.XplaneData.HasStrobeLightsOn)
                            {
                                mUserAircraftData.StrobeLightsOn = wrapper.XplaneData.StrobeLightsOn;
                            }
                            if (wrapper.XplaneData.HasTaxiLightsOn)
                            {
                                mUserAircraftData.TaxiLightsOn = wrapper.XplaneData.TaxiLightsOn;
                            }

                            // controls
                            if (wrapper.XplaneData.HasFlaps)
                            {
                                mUserAircraftData.Flaps = wrapper.XplaneData.Flaps;
                            }
                            if (wrapper.XplaneData.HasSpeedBrakes)
                            {
                                mUserAircraftData.SpeedBrakeDeployed = wrapper.XplaneData.SpeedBrakes > 0;
                            }
                            if (wrapper.XplaneData.HasGearDown)
                            {
                                mUserAircraftData.GearDown = wrapper.XplaneData.GearDown;
                            }

                            if (wrapper.XplaneData.HasEngineCount)
                            {
                                mUserAircraftData.EngineCount = wrapper.XplaneData.EngineCount;
                            }
                            if (wrapper.XplaneData.HasEngine1Running)
                            {
                                mUserAircraftData.Engine1Running = wrapper.XplaneData.Engine1Running;
                            }
                            if (wrapper.XplaneData.HasEngine2Running)
                            {
                                mUserAircraftData.Engine2Running = wrapper.XplaneData.Engine2Running;
                            }
                            if (wrapper.XplaneData.HasEngine3Running)
                            {
                                mUserAircraftData.Engine3Running = wrapper.XplaneData.Engine3Running;
                            }
                            if (wrapper.XplaneData.HasEngine4Running)
                            {
                                mUserAircraftData.Engine4Running = wrapper.XplaneData.Engine4Running;
                            }

                            if (wrapper.XplaneData.HasOnGround)
                            {
                                mUserAircraftData.OnGround = wrapper.XplaneData.OnGround;
                            }
                            if (wrapper.XplaneData.HasReplayMode)
                            {
                                mUserAircraftData.ReplayModeEnabled = wrapper.XplaneData.ReplayMode;
                            }

                            mConnectionHeartbeats.Add(wrapper.Timestamp.ToDateTime());
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

        [Obsolete("Use SendProtobufArray")]
        public void SendMessage(string msg)
        {

        }

        private void WhosOnlineListRefresh_Tick(object sender, EventArgs e)
        {
            List<WhosOnlineList> temp = new List<WhosOnlineList>();
            foreach (var controller in mControllers)
            {
                temp.Add(new WhosOnlineList
                {
                    Callsign = controller.Callsign,
                    XplaneFrequency = controller.NormalizedFrequency + 100000,
                    Frequency = (controller.NormalizedFrequency.FsdFrequencyToHertz() / 1000000.0).ToString("0.000"),
                    RealName = controller.RealName
                });
            }

            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.WhosOnline,
                Timestamp = DateTime.Now,
                Data = temp.OrderBy(a => a.Callsign)
            }.ToJSON());
        }

        private void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (mConnectionHeartbeats.Count > 0)
            {
                TimeSpan span = DateTime.UtcNow - mConnectionHeartbeats.Last();
                if (span.TotalSeconds < 15)
                {
                    if (!mReceivedInitialHandshake)
                    {
                        RaiseSimConnectionStateChanged(this, new ClientEventArgs<bool>(false));
                        RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));

                        RequestPluginVersion();
                        RequestPluginHash();
                    }
                    else
                    {
                        RaiseSimConnectionStateChanged(this, new ClientEventArgs<bool>(true));
                        if (mValidCsl && !mInvalidPluginVersionShown)
                        {
                            RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(true));
                        }
                    }
                    mConnectionHeartbeats.RemoveAt(0);
                }
                else
                {
                    mReceivedInitialHandshake = false;
                    RaiseSimConnectionStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
                    RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
                }
            }
            else
            {
                mReceivedInitialHandshake = false;
                RaiseSimConnectionStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
                RaiseConnectButtonStateChanged?.Invoke(this, new ClientEventArgs<bool>(false));
            }
        }

        private void DataRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (mUserAircraftData.TransponderMode >= 2)
            {
                RaiseTransponderModeChanged?.Invoke(this, new ClientEventArgs<bool>(true));
            }
            else
            {
                RaiseTransponderModeChanged?.Invoke(this, new ClientEventArgs<bool>(false));
            }

            UserAircraftRadioStack radio = new UserAircraftRadioStack
            {
                ComAudioSelection = mRadioStackState.ComAudioSelection,
                Com1ActiveFreq = mRadioStackState.Com1ActiveFreq,
                Com1StandbyFreq = mRadioStackState.Com1StandbyFreq,
                Com1Power = mRadioStackState.Com1Power,
                Com1Listening = mRadioStackState.Com1Listening,
                Com2ActiveFreq = mRadioStackState.Com2ActiveFreq,
                Com2StandbyFreq = mRadioStackState.Com2StandbyFreq,
                Com2Power = mRadioStackState.Com2Power,
                Com2Listening = mRadioStackState.Com2Listening,
                Com1ForceRx = mRadioStackState.Com1ForceRx,
                Com1ForceTx = mRadioStackState.Com1ForceTx,
                Com2ForceRx = mRadioStackState.Com2ForceRx,
                Com2ForceTx = mRadioStackState.Com2ForceTx,
                AvionicsPower = mRadioStackState.AvionicsPower
            };

            RaiseUserAircraftDataUpdated?.Invoke(this, new UserAircraftDataChanged(mUserAircraftData));
            RaiseRadioStackStateChanged?.Invoke(this, new RadioStackStateChanged(radio));
        }

        [EventSubscription(EventTopics.RadioVolumeChanged, typeof(OnUserInterfaceAsync))]
        public void OnVolumeLevelsChanged(object sender, RadioVolumeChanged e)
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

        [EventSubscription(EventTopics.OverrideRadioStackState, typeof(OnUserInterfaceAsync))]
        public void OnOverrideComStatus(object sender, OverrideRadioStackState e)
        {
            mRadioStackState.Com1ForceRx = e.Com1Rx;
            mRadioStackState.Com1ForceTx = e.Com1Tx;
            mRadioStackState.Com2ForceRx = e.Com2Rx;
            mRadioStackState.Com2ForceTx = e.Com2Tx;
        }

        //[EventSubscription(EventTopics.SendXplaneCommand, typeof(OnUserInterfaceAsync))]
        //public void OnXplaneCommand(object sender, ClientEventArgs<XPlaneCommand> e)
        //{
        //    mXplaneConnector.SendCommand(e.Value);
        //}

        //[EventSubscription(EventTopics.SetXplaneDataRefValue, typeof(OnUserInterfaceAsync))]
        //public void OnSetXplaneDataRefValue(object sender, DataRefEventArgs e)
        //{
        //    mXplaneConnector.SetDataRefValue(e.DataRef, e.Value);
        //}

        [EventSubscription(EventTopics.SimulatorMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnSimulatorMessageSent(object sender, SimulatorMessageSent e)
        {
            //SendMessage(new XplaneConnect
            //{
            //    Type = XplaneConnect.MessageType.RadioMessage,
            //    Timestamp = DateTime.Now,
            //    Data = new XplaneTextMessage
            //    {
            //        Message = e.Text,
            //        Red = e.Red,
            //        Green = e.Green,
            //        Blue = e.Blue,
            //        Direct = e.PriorityMessage
            //    }
            //}.ToJSON());
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChanged e)
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
        public void OnNetworkConnected(object sender, NetworkConnected e)
        {
            dynamic data = new ExpandoObject();
            data.OurCallsign = e.ConnectInfo.Callsign;

            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.NetworkConnected,
                Timestamp = DateTime.Now,
                Data = data
            }.ToJSON());

            mDisconnectMessageSent = false;
            mWhosOnlineListRefresh.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnected e)
        {
            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.WhosOnline,
                Timestamp = DateTime.Now,
                Data = null
            }.ToJSON());

            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.RemoveAllPlanes,
                Timestamp = DateTime.Now,
                Data = null
            }.ToJSON());

            if (!mDisconnectMessageSent)
            {
                RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent("Disconnected from network.", true));
                mDisconnectMessageSent = true;
            }

            mWhosOnlineListRefresh.Stop();
            mControllers.Clear();
        }

        [EventSubscription(EventTopics.AddControllerRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnControllerAdded(object sender, ControllerUpdateReceived e)
        {
            if (!mControllers.Contains(e.Controller))
            {
                mControllers.Add(e.Controller);
            }
        }

        [EventSubscription(EventTopics.DeleteControllerRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeleteControllerRequestReceived(object sender, ControllerUpdateReceived e)
        {
            if (mControllers.Contains(e.Controller))
            {
                mControllers.Remove(e.Controller);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void OnControllerFrequencyChanged(object sender, ControllerUpdateReceived e)
        {
            var controller = mControllers.FirstOrDefault(o => o.Callsign == e.Controller.Callsign);
            if (controller != null)
            {
                controller.Frequency = e.Controller.Frequency;
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceived e)
        {
            var controller = mControllers.FirstOrDefault(o => o.Callsign == e.From);
            if (controller != null)
            {
                controller.RealName = e.Data;
            }
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, PrivateMessageReceived e)
        {
            if (e.From != "SERVER")
            {
                dynamic data = new ExpandoObject();
                data.From = e.From.ToUpper();
                data.Message = e.Message;

                SendMessage(new XplaneConnect
                {
                    Type = XplaneConnect.MessageType.PrivateMessageReceived,
                    Timestamp = DateTime.Now,
                    Data = data
                }.ToJSON());
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSent e)
        {
            dynamic data = new ExpandoObject();
            data.To = e.To.ToUpper();
            data.Message = e.Message;

            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.PrivateMessageSent,
                Timestamp = DateTime.Now,
                Data = data
            }.ToJSON());
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

        [EventSubscription(EventTopics.MicrophoneInputLevelChanged, typeof(OnUserInterfaceAsync))]
        public void MicrophoneInputLevelChanged(object sender, ClientEventArgs<float> e)
        {
            //var dr = new DataRefElement
            //{
            //    DataRef = "xpilot/audio/vu"
            //};
            //mXplaneConnector.SetDataRefValue(dr, e.Value);
        }

        public void SetTransponderCode(int code)
        {
            
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
            
        }

        public void PlanePoseChanged(NetworkAircraft plane, NetworkAircraftPose pose)
        {
            
        }

        public void PlaneConfigChanged(NetworkAircraft plane, NetworkAircraftConfig config)
        {
            
        }

        public void RemovePlane(NetworkAircraft plane)
        {
            
        }
    }
}