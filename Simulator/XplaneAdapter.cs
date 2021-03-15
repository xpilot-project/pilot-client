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

        [EventPublication(EventTopics.AircraftAddedToSimulator)]
        public event EventHandler<GenericEventArgs<string>> AircraftAddedToSimulator;

        private readonly INetworkManager mFsdManager;
        private readonly IAppConfig mConfig;

        private NetMQQueue<byte[]> mMessageQueue;
        private NetMQPoller mPoller;
        private DealerSocket mDealerSocket;
        private List<DealerSocket> mVisualDealerSockets;

        private Timer mConnectionTimer;
        private Stack<DateTime> mConnectionHeartbeats;
        private List<int> mTunedFrequencies;

        private Aircrafts.UserAircraftData mUserAircraftData;
        private Aircrafts.UserAircraftConfigData mUserAircraftConfigData;
        private RadioStackState mRadioStackState;

        private string mSimulatorIP = "127.0.0.1";
        private bool mValidCsl = false;
        private bool mValidPluginVersion = false;
        private bool mInvalidPluginMessageShown = false;
        private bool mCslConfigurationErrorShown = false;

        public bool ValidSimConnection => mValidCsl && mValidPluginVersion;

        public List<int> TunedFrequencies => mTunedFrequencies;

        public XplaneAdapter(IEventBroker broker, IAppConfig config, INetworkManager fsdManager) : base(broker)
        {
            mConfig = config;
            mFsdManager = fsdManager;

            mVisualDealerSockets = null;
            mConnectionHeartbeats = new Stack<DateTime>();
            mTunedFrequencies = new List<int>();

            if (mConfig.VisualClientIPs.Count > 0)
            {
                DealerSocket visualDealerSocket = null;
                foreach (string mIP in mConfig.VisualClientIPs)
                {
                    visualDealerSocket = new DealerSocket("tcp://" + mIP + ":" + mConfig.SimulatorPort);
                    visualDealerSocket.Options.Identity = Encoding.UTF8.GetBytes("CLIENT");
                    visualDealerSocket.Options.TcpKeepalive = true;
                    if (mVisualDealerSockets == null)
                    {
                        mVisualDealerSockets = new List<DealerSocket>();
                    }
                    mVisualDealerSockets.Add(visualDealerSocket);
                }
            }

            if (!string.IsNullOrEmpty(mConfig.SimulatorIP))
            {
                mSimulatorIP = mConfig.SimulatorIP;
            }

            mMessageQueue = new NetMQQueue<byte[]>();

            mDealerSocket = new DealerSocket("tcp://" + mSimulatorIP + ":" + mConfig.SimulatorPort);
            mDealerSocket.Options.TcpKeepalive = true;
            mDealerSocket.Options.Identity = Encoding.UTF8.GetBytes("CLIENT");
            mDealerSocket.ReceiveReady += DealerSocket_ReceiveReady;

            mPoller = new NetMQPoller { mDealerSocket, mMessageQueue };
            mPoller.RunAsync();

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

            mUserAircraftData = new Aircrafts.UserAircraftData();
            mUserAircraftConfigData = new Aircrafts.UserAircraftConfigData();
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

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            NetworkDisconnected();
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
            RequestPluginInformation();
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

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            byte[] bytes = e.Socket.ReceiveFrameBytes();
            if (bytes.Length > 0)
            {
                var wrapper = Wrapper.Parser.ParseFrom(bytes);

                if (wrapper.Timestamp != null)
                {
                    mConnectionHeartbeats.Push(wrapper.Timestamp.ToDateTime());
                }

                switch (wrapper.MsgCase)
                {
                    case Wrapper.MsgOneofCase.PluginInformation:
                        {
                            if (wrapper.PluginInformation.HasVersion)
                            {
                                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                                string compactedVersion = string.Format($"{v.Major}{v.Minor}{v.Build}");
                                int versionInt = int.Parse(compactedVersion);
                                if (v.Build < 10) versionInt *= 10;
                                if (wrapper.PluginInformation.Version == versionInt)
                                {
                                    mValidPluginVersion = true;
                                    mInvalidPluginMessageShown = false;
                                }
                                else
                                {
                                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
                                    if (!mInvalidPluginMessageShown)
                                    {
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot."));
                                        PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                        mInvalidPluginMessageShown = true;
                                    }
                                }
                            }
                            if (wrapper.PluginInformation.HasHash)
                            {
                                mFsdManager.SetPluginHash(wrapper.PluginInformation.Hash);
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.CslValidation:
                        {
                            if (wrapper.CslValidation.HasValid)
                            {
                                mValidCsl = wrapper.CslValidation.Valid;
                                if (!wrapper.CslValidation.Valid)
                                {
                                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);
                                    if (!mCslConfigurationErrorShown)
                                    {
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Settings). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models."));
                                        PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                        mCslConfigurationErrorShown = true;
                                    }
                                }
                                else
                                {
                                    mCslConfigurationErrorShown = false;
                                }
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.PlaneAddedToSim:
                        {
                            AircraftAddedToSimulator?.Invoke(this, new GenericEventArgs<string>(wrapper.PlaneAddedToSim.Callsign));
                        }
                        break;
                    case Wrapper.MsgOneofCase.RadioStack:
                        {
                            if (wrapper.RadioStack.HasPttPressed)
                            {
                                PushToTalkStateChanged?.Invoke(this, new PushToTalkStateChangedEventArgs(wrapper.RadioStack.PttPressed));
                            }

                            if (wrapper.RadioStack.HasAudioComSelection)
                            {
                                mRadioStackState.Com1TransmitEnabled = (uint)wrapper.RadioStack.AudioComSelection == 6;
                                mRadioStackState.Com2TransmitEnabled = (uint)wrapper.RadioStack.AudioComSelection == 7;
                            }

                            // com1
                            if (wrapper.RadioStack.HasCom1Freq)
                            {
                                mRadioStackState.Com1ActiveFrequency = ((uint)wrapper.RadioStack.Com1Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.RadioStack.HasCom1AudioSelection)
                            {
                                mRadioStackState.Com1ReceiveEnabled = wrapper.RadioStack.Com1AudioSelection;
                            }
                            if (wrapper.RadioStack.HasCom1Volume)
                            {
                                float val = wrapper.RadioStack.Com1Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(1, val));
                            }

                            // com2
                            if (wrapper.RadioStack.HasCom2Freq)
                            {
                                mRadioStackState.Com2ActiveFrequency = ((uint)wrapper.RadioStack.Com2Freq * 1000).Normalize25KhzFrequency();
                            }
                            if (wrapper.RadioStack.HasCom2AudioSelection)
                            {
                                mRadioStackState.Com2ReceiveEnabled = wrapper.RadioStack.Com2AudioSelection;
                            }
                            if (wrapper.RadioStack.HasCom2Volume)
                            {
                                float val = wrapper.RadioStack.Com2Volume;
                                if (val > 1.0f) val = 1.0f;
                                if (val < 0.0f) val = 0.0f;
                                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(2, val));
                            }

                            if (wrapper.RadioStack.HasAvionicsPowerOn)
                            {
                                mRadioStackState.AvionicsPowerOn = wrapper.RadioStack.AvionicsPowerOn;
                            }

                            if (wrapper.RadioStack.HasTransponderCode)
                            {
                                mRadioStackState.TransponderCode = (ushort)wrapper.RadioStack.TransponderCode;
                            }
                            if (wrapper.RadioStack.HasTransponderMode)
                            {
                                mRadioStackState.SquawkingModeC = wrapper.RadioStack.TransponderMode >= 2;
                            }
                            if (wrapper.RadioStack.HasTransponderIdent)
                            {
                                mRadioStackState.SquawkingIdent = wrapper.RadioStack.TransponderIdent;
                            }

                            RadioStackStateChanged?.Invoke(this, new RadioStackStateChangedEventArgs(mRadioStackState));
                            UpdateTunedFrequencies();
                        }
                        break;
                    case Wrapper.MsgOneofCase.UserAircraftData:
                        {
                            if (wrapper.UserAircraftData.HasVelocityLatitude)
                            {
                                mUserAircraftData.LatitudeVelocity = Math.Round(wrapper.UserAircraftData.VelocityLatitude, 4);
                            }
                            if (wrapper.UserAircraftData.HasVelocityAltitude)
                            {
                                mUserAircraftData.AltitudeVelocity = Math.Round(wrapper.UserAircraftData.VelocityAltitude, 4);
                            }
                            if (wrapper.UserAircraftData.HasVelocityLongitude)
                            {
                                mUserAircraftData.LongitudeVelocity = Math.Round(wrapper.UserAircraftData.VelocityLongitude, 4);
                            }

                            if (wrapper.UserAircraftData.HasVelocityPitch)
                            {
                                mUserAircraftData.PitchVelocity = wrapper.UserAircraftData.VelocityPitch;
                            }
                            if (wrapper.UserAircraftData.HasVelocityHeading)
                            {
                                mUserAircraftData.HeadingVelocity = wrapper.UserAircraftData.VelocityHeading;
                            }
                            if (wrapper.UserAircraftData.HasVelocityBank)
                            {
                                mUserAircraftData.BankVelocity = wrapper.UserAircraftData.VelocityBank;
                            }

                            if (wrapper.UserAircraftData.HasLatitude)
                            {
                                mUserAircraftData.Latitude = wrapper.UserAircraftData.Latitude;
                            }
                            if (wrapper.UserAircraftData.HasLongitude)
                            {
                                mUserAircraftData.Longitude = wrapper.UserAircraftData.Longitude;
                            }
                            if (wrapper.UserAircraftData.HasAltitude)
                            {
                                mUserAircraftData.AltitudeTrue = wrapper.UserAircraftData.Altitude * 3.28084;
                            }
                            if (wrapper.UserAircraftData.HasPressureAltitude)
                            {
                                mUserAircraftData.AltitudePressure = wrapper.UserAircraftData.PressureAltitude * 3.28084;
                            }
                            if (wrapper.UserAircraftData.HasYaw)
                            {
                                mUserAircraftData.Heading = wrapper.UserAircraftData.Yaw;
                            }
                            if (wrapper.UserAircraftData.HasPitch)
                            {
                                mUserAircraftData.Pitch = wrapper.UserAircraftData.Pitch;
                            }
                            if (wrapper.UserAircraftData.HasRoll)
                            {
                                mUserAircraftData.Bank = wrapper.UserAircraftData.Roll;
                            }
                            if (wrapper.UserAircraftData.HasGroundSpeed)
                            {
                                mUserAircraftData.SpeedGround = wrapper.UserAircraftData.GroundSpeed;
                            }

                            UserAircraftDataUpdated?.Invoke(this, new UserAircraftDataUpdatedEventArgs(mUserAircraftData));
                        }
                        break;
                    case Wrapper.MsgOneofCase.UserAircraftConfig:
                        {
                            // lights
                            if (wrapper.UserAircraftConfig.HasBeaconLightsOn)
                            {
                                mUserAircraftConfigData.BeaconOn = wrapper.UserAircraftConfig.BeaconLightsOn;
                            }
                            if (wrapper.UserAircraftConfig.HasLandingLightsOn)
                            {
                                mUserAircraftConfigData.LandingLightsOn = wrapper.UserAircraftConfig.LandingLightsOn;
                            }
                            if (wrapper.UserAircraftConfig.HasNavLightsOn)
                            {
                                mUserAircraftConfigData.NavLightOn = wrapper.UserAircraftConfig.NavLightsOn;
                            }
                            if (wrapper.UserAircraftConfig.HasStrobeLightsOn)
                            {
                                mUserAircraftConfigData.StrobesOn = wrapper.UserAircraftConfig.StrobeLightsOn;
                            }
                            if (wrapper.UserAircraftConfig.HasTaxiLightsOn)
                            {
                                mUserAircraftConfigData.TaxiLightsOn = wrapper.UserAircraftConfig.TaxiLightsOn;
                            }

                            // controls
                            if (wrapper.UserAircraftConfig.HasFlaps)
                            {
                                mUserAircraftConfigData.FlapsRatio = wrapper.UserAircraftConfig.Flaps;
                            }
                            if (wrapper.UserAircraftConfig.HasSpeedBrakes)
                            {
                                mUserAircraftConfigData.SpoilersRatio = wrapper.UserAircraftConfig.SpeedBrakes;
                            }
                            if (wrapper.UserAircraftConfig.HasGearDown)
                            {
                                mUserAircraftConfigData.GearDown = wrapper.UserAircraftConfig.GearDown;
                            }
                            if (wrapper.UserAircraftConfig.HasOnGround)
                            {
                                mUserAircraftConfigData.OnGround = wrapper.UserAircraftConfig.OnGround;
                            }

                            if (wrapper.UserAircraftConfig.HasEngineCount)
                            {
                                mUserAircraftConfigData.EngineCount = wrapper.UserAircraftConfig.EngineCount;
                            }
                            if (wrapper.UserAircraftConfig.HasEngine1Running)
                            {
                                mUserAircraftConfigData.Engine1Running = wrapper.UserAircraftConfig.Engine1Running;
                            }
                            if (wrapper.UserAircraftConfig.HasEngine2Running)
                            {
                                mUserAircraftConfigData.Engine2Running = wrapper.UserAircraftConfig.Engine2Running;
                            }
                            if (wrapper.UserAircraftConfig.HasEngine3Running)
                            {
                                mUserAircraftConfigData.Engine3Running = wrapper.UserAircraftConfig.Engine3Running;
                            }
                            if (wrapper.UserAircraftConfig.HasEngine4Running)
                            {
                                mUserAircraftConfigData.Engine4Running = wrapper.UserAircraftConfig.Engine4Running;
                            }

                            UserAircraftConfigDataUpdated?.Invoke(this, new UserAircraftConfigDataUpdatedEventArgs(mUserAircraftConfigData));
                        }
                        break;
                }
            }
        }

        private void UpdateTunedFrequencies()
        {
            mTunedFrequencies.Clear();
            if (mRadioStackState.Com1TransmitEnabled)
            {
                mTunedFrequencies.Add(mRadioStackState.Com1ActiveFrequency.Normalize25KhzFrequency().MatchNetworkFormat());
                if (!mTunedFrequencies.Contains(mRadioStackState.Com1ActiveFrequency.UnNormalize25KhzFrequency().MatchNetworkFormat()))
                {
                    mTunedFrequencies.Add(mRadioStackState.Com1ActiveFrequency.UnNormalize25KhzFrequency().MatchNetworkFormat());
                }
            }
            if (mRadioStackState.Com2TransmitEnabled)
            {
                mTunedFrequencies.Add(mRadioStackState.Com2ActiveFrequency.Normalize25KhzFrequency().MatchNetworkFormat());
                if (!mTunedFrequencies.Contains(mRadioStackState.Com2ActiveFrequency.UnNormalize25KhzFrequency().MatchNetworkFormat()))
                {
                    mTunedFrequencies.Add(mRadioStackState.Com2ActiveFrequency.UnNormalize25KhzFrequency().MatchNetworkFormat());
                }
            }
        }

        private void RequestPluginInformation()
        {
            var msg = new Wrapper
            {
                PluginInformation = new PluginInformation()
            };
            SendProtobufArray(msg.ToByteArray());
        }

        private void RequestCslValidation()
        {
            var msg = new Wrapper
            {
                CslValidation = new CslValidation()
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
            if(mConnectionHeartbeats.Count > 0)
            {
                TimeSpan span = DateTime.UtcNow - mConnectionHeartbeats.Pop();
                if(span.TotalMilliseconds < 15000)
                {
                    if(ValidSimConnection)
                    {
                        SimulatorConnected?.Invoke(this, EventArgs.Empty);
                        ConnectButtonEnabled?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                        ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);

                        RequestCslValidation();
                        RequestPluginInformation();
                    }
                }
                else
                {
                    SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                    ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);

                    RequestCslValidation();
                    RequestPluginInformation();
                }
            }
            else
            {
                SimulatorDisconnected?.Invoke(this, EventArgs.Empty);
                ConnectButtonDisabled?.Invoke(this, EventArgs.Empty);

                RequestCslValidation();
                RequestPluginInformation();
            }
        }

        public void SetTransponderCode(int code)
        {
            var msg = new Wrapper
            {
                SetTransponder = new SetTransponder()
            };
            msg.SetTransponder.Code = code;
            SendProtobufArray(msg.ToByteArray());
        }

        public void EnableTransponderModeC(bool enabled)
        {
            var msg = new Wrapper
            {
                SetTransponder = new SetTransponder()
            };
            msg.SetTransponder.ModeC = enabled;
            SendProtobufArray(msg.ToByteArray());
        }

        public void TriggerTransponderIdent()
        {
            var msg = new Wrapper
            {
                SetTransponder = new SetTransponder()
            };
            msg.SetTransponder.Ident = true;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SetRadioFrequency(int radio, uint freq)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.Frequency = (int)freq;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SetRadioTransmit(int radio)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.TransmitEnabled = true;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SetRadioReceive(int radio, bool enabled)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.ReceiveEnabled = enabled;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SetLoginStatus(bool connected)
        {

        }

        public void PlaneConfigChanged(AirplaneConfig config)
        {
            var msg = new Wrapper
            {
                AirplaneConfig = config
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

        public void PlaneConfigChanged(Aircraft aircraft)
        {
            var cfg = new AirplaneConfig
            {
                Callsign = aircraft.Callsign,
                IsFullConfig = aircraft.Configuration.IsFullData.Value,
                EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning
            };
            if (aircraft.Configuration.FlapsPercent.HasValue)
            {
                cfg.Flaps = (float)(aircraft.Configuration.FlapsPercent.Value / 100.0f);
            }
            if (aircraft.Configuration.OnGround.HasValue)
            {
                cfg.OnGround = aircraft.Configuration.OnGround.Value;
            }
            if (aircraft.Configuration.GearDown.HasValue)
            {
                cfg.GearDown = aircraft.Configuration.GearDown.Value;
            }
            if (aircraft.Configuration.Lights != null)
            {
                cfg.Lights = new AirplaneConfig.Types.AirplaneConfigLights();
                if (aircraft.Configuration.Lights.BeaconOn.HasValue)
                {
                    cfg.Lights.BeaconLightsOn = aircraft.Configuration.Lights.BeaconOn.Value;
                }
                if (aircraft.Configuration.Lights.NavOn.HasValue)
                {
                    cfg.Lights.NavLightsOn = aircraft.Configuration.Lights.NavOn.Value;
                }
                if (aircraft.Configuration.Lights.LandingOn.HasValue)
                {
                    cfg.Lights.LandingLightsOn = aircraft.Configuration.Lights.LandingOn.Value;
                }
                if (aircraft.Configuration.Lights.TaxiOn.HasValue)
                {
                    cfg.Lights.TaxiLightsOn = aircraft.Configuration.Lights.TaxiOn.Value;
                }
                if (aircraft.Configuration.Lights.StrobesOn.HasValue)
                {
                    cfg.Lights.StrobeLightsOn = aircraft.Configuration.Lights.StrobesOn.Value;
                }
            }

            var msg = new Wrapper
            {
                AirplaneConfig = cfg
            };
            SendProtobufArray(msg.ToByteArray());
        }

        public void AddPlane(Aircraft aircraft)
        {
            var msg = new Wrapper
            {
                AddPlane = new AddPlane()
            };
            msg.AddPlane.Callsign = aircraft.Callsign;
            msg.AddPlane.Equipment = aircraft.TypeCode ?? "";
            msg.AddPlane.Airline = aircraft.Airline ?? "";
            msg.AddPlane.VisualState = new AddPlane.Types.AircraftVisualState
            {
                Latitude = aircraft.RemoteVisualState.Location.Lat,
                Longitude = aircraft.RemoteVisualState.Location.Lon,
                Altitude = aircraft.RemoteVisualState.Altitude,
                Pitch = aircraft.RemoteVisualState.Pitch,
                Heading = aircraft.RemoteVisualState.Heading,
                Bank = aircraft.RemoteVisualState.Bank
            };
            SendProtobufArray(msg.ToByteArray());
        }

        public void DeleteAircraft(Aircraft aircraft)
        {
            var msg = new Wrapper
            {
                DeletePlane = new DeletePlane()
            };
            msg.DeletePlane.Callsign = aircraft.Callsign;
            SendProtobufArray(msg.ToByteArray());
        }

        public void ChangeModel(Aircraft aircraft)
        {
            var msg = new Wrapper
            {
                ChangePlaneModel = new ChangePlaneModel()
            };
            msg.ChangePlaneModel.Callsign = aircraft.Callsign;
            msg.ChangePlaneModel.Equipment = aircraft.TypeCode;
            msg.ChangePlaneModel.Airline = aircraft.Airline;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SendFastPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, VelocityVector positionalVelocityVector, VelocityVector rotationalVelocityVector)
        {
            var msg = new Wrapper
            {
                FastPositionUpdate = new Protobuf.FastPositionUpdate()
            };
            msg.FastPositionUpdate.Callsign = aircraft.Callsign;
            msg.FastPositionUpdate.Latitude = visualState.Location.Lat;
            msg.FastPositionUpdate.Longitude = visualState.Location.Lon;
            msg.FastPositionUpdate.Altitude = visualState.Altitude;
            msg.FastPositionUpdate.Heading = visualState.Heading;
            msg.FastPositionUpdate.Pitch = visualState.Pitch;
            msg.FastPositionUpdate.Bank = visualState.Bank;
            msg.FastPositionUpdate.VelocityLongitude = positionalVelocityVector.X;
            msg.FastPositionUpdate.VelocityAltitude = positionalVelocityVector.Y;
            msg.FastPositionUpdate.VelocityLatitude = positionalVelocityVector.Z;
            msg.FastPositionUpdate.VelocityPitch = rotationalVelocityVector.X;
            msg.FastPositionUpdate.VelocityHeading = rotationalVelocityVector.Y;
            msg.FastPositionUpdate.VelocityBank = rotationalVelocityVector.Z;
            SendProtobufArray(msg.ToByteArray());
        }

        public void SendSlowPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, double groundSpeed)
        {
            var msg = new Wrapper
            {
                PositionUpdate = new Protobuf.PositionUpdate()
            };
            msg.PositionUpdate.Callsign = aircraft.Callsign;
            msg.PositionUpdate.Latitude = visualState.Location.Lat;
            msg.PositionUpdate.Longitude = visualState.Location.Lon;
            msg.PositionUpdate.Altitude = visualState.Altitude;
            msg.PositionUpdate.Pitch = visualState.Pitch;
            msg.PositionUpdate.Heading = visualState.Heading;
            msg.PositionUpdate.Bank = visualState.Bank;
            SendProtobufArray(msg.ToByteArray());
        }
    }
}