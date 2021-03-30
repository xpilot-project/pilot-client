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
using System.Drawing;
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
using Vatsim.Xpilot.Controllers;

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

        [EventPublication(EventTopics.ReplayModeEnabled)]
        public event EventHandler<EventArgs> ReplayModeDetected;

        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSentEventArgs> PrivateMessageSent;

        [EventPublication(EventTopics.RadioMessageSent)]
        public event EventHandler<RadioMessageSentEventArgs> RadioMessageSent;

        private readonly INetworkManager mNetworkManager;
        private readonly IControllerAtisManager mControllerAtisManager;
        private readonly IAppConfig mConfig;

        private readonly NetMQQueue<byte[]> mMessageQueue;
        private readonly NetMQPoller mPoller;
        private readonly DealerSocket mDealerSocket;
        private readonly List<DealerSocket> mVisualDealerSockets;

        private readonly Timer mConnectionTimer;
        private readonly Stack<DateTime> mConnectionHeartbeats;
        private readonly List<int> mTunedFrequencies;

        private UserAircraftData mUserAircraftData;
        private UserAircraftConfigData mUserAircraftConfigData;
        private RadioStackState mRadioStackState;

        private string mSimulatorIP = "127.0.0.1";
        private bool mValidCsl = false;
        private bool mValidPluginVersion = false;
        private bool mInvalidPluginMessageShown = false;
        private bool mCslConfigurationErrorShown = false;
        private bool mIsPaused = false;

        public bool ValidSimConnection => mValidCsl && mValidPluginVersion;
        public List<int> TunedFrequencies => mTunedFrequencies;

        public XplaneAdapter(IEventBroker broker, IAppConfig config, INetworkManager networkManager, IControllerAtisManager controllerAtisManager) : base(broker)
        {
            mConfig = config;
            mNetworkManager = networkManager;
            mControllerAtisManager = controllerAtisManager;

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
                    break;
                case 2:
                    break;
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            NetworkConnected(e.ConnectInfo.Callsign);
            AddMessageToConsole("Connected to network", Color.Yellow);
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            NetworkDisconnected();
            AddMessageToConsole("Disconnected from network", Color.Yellow);
        }

        [EventSubscription(EventTopics.ServerMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnServerMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AddMessageToConsole($"[SERVER]: {e.Data}", Color.SeaGreen);
        }

        [EventSubscription(EventTopics.BroadcastMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnBroadcastMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AddMessageToConsole($"[BROADCAST]: {e.Data}", Color.Orange);
        }

        [EventSubscription(EventTopics.WallopSent, typeof(OnUserInterfaceAsync))]
        public void OnWallopSent(object sender, WallopSentEventArgs e)
        {
            AddMessageToConsole($"[WALLOP]: {e.OurCallsign}: {e.Message}", Color.Red);
        }

        [EventSubscription(EventTopics.MetarReceived, typeof(OnUserInterfaceAsync))]
        public void OnMetarReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AddMessageToConsole(e.Data, Color.LimeGreen);
        }

        [EventSubscription(EventTopics.RequestedAtisReceived, typeof(OnUserInterfaceAsync))]
        public void OnRequestAtisReceived(object sender, RequestedAtisReceivedEventArgs e)
        {
            AddMessageToConsole($"{e.From}: ATIS", Color.LimeGreen);
            foreach (string line in e.Lines)
            {
                AddMessageToConsole(line, Color.LimeGreen);
            }
        }

        [EventSubscription(EventTopics.RadioMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnRadioMessageSent(object sender, RadioMessageSentEventArgs e)
        {
            AddMessageToConsole($"{e.OurCallsign}: {e.Message}", Color.Cyan);
        }

        [EventSubscription(EventTopics.RadioMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnRadioMessageReceived(object sender, RadioMessageReceivedEventArgs e)
        {
            AddMessageToConsole($"{e.From}: {e.Data}", e.IsDirect ? Color.White : Color.Gray);
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
                SendProtobufArray(msg);
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
            SendProtobufArray(msg);
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

        private void AddMessageToConsole(string message, Color color)
        {
            var msg = new Wrapper
            {
                RadioMessageReceived = new Protobuf.RadioMessageReceived()
            };
            msg.RadioMessageReceived.Message = message;
            msg.RadioMessageReceived.Color = color.RgbToInt();
            SendProtobufArray(msg);
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
                                string compactedVersion = string.Format($"{v.Major}{v.Minor}{v.Build}{v.Revision}");
                                int versionInt = int.Parse(compactedVersion);
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
                                        string msg = "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot.";
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, msg));
                                        AddMessageToConsole(msg, Color.Red);
                                        PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                        mInvalidPluginMessageShown = true;
                                    }
                                }
                            }
                            if (wrapper.PluginInformation.HasHash)
                            {
                                mNetworkManager.SetPluginHash(wrapper.PluginInformation.Hash);
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
                                        string msg = "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Settings). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models.";
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, msg));
                                        PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
                                        AddMessageToConsole(msg, Color.Red);
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
                    case Wrapper.MsgOneofCase.TriggerDisconnect:
                        if (wrapper.TriggerDisconnect.HasReason)
                        {
                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, wrapper.TriggerDisconnect.Reason));
                        }
                        mNetworkManager.Disconnect();
                        break;
                    case Wrapper.MsgOneofCase.RequestStationInfo:
                        if (wrapper.RequestStationInfo.HasCallsign)
                        {
                            mControllerAtisManager.RequestControllerAtis(wrapper.RequestStationInfo.Callsign);
                        }
                        break;
                    case Wrapper.MsgOneofCase.PrivateMessageSent:
                        {
                            if (wrapper.PrivateMessageSent.HasMessage && wrapper.PrivateMessageSent.HasTo)
                            {
                                PrivateMessageSent?.Invoke(this, new PrivateMessageSentEventArgs(mNetworkManager.OurCallsign, wrapper.PrivateMessageSent.To, wrapper.PrivateMessageSent.Message));
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.RadioMessageSent:
                        {
                            if (wrapper.RadioMessageSent.HasMessage)
                            {
                                RadioMessageSent?.Invoke(this, new RadioMessageSentEventArgs(mNetworkManager.OurCallsign, wrapper.RadioMessageSent.Message));
                            }
                        }
                        break;
                    case Wrapper.MsgOneofCase.XplaneData:
                        {
                            // radio stack
                            if (wrapper.XplaneData.RadioStack != null)
                            {
                                if (wrapper.XplaneData.RadioStack.HasPttPressed)
                                {
                                    PushToTalkStateChanged?.Invoke(this, new PushToTalkStateChangedEventArgs(wrapper.XplaneData.RadioStack.PttPressed));
                                }
                                if (wrapper.XplaneData.RadioStack.HasAudioComSelection)
                                {
                                    mRadioStackState.Com1TransmitEnabled = (uint)wrapper.XplaneData.RadioStack.AudioComSelection == 6;
                                    mRadioStackState.Com2TransmitEnabled = (uint)wrapper.XplaneData.RadioStack.AudioComSelection == 7;
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom1Freq)
                                {
                                    mRadioStackState.Com1ActiveFrequency = ((uint)wrapper.XplaneData.RadioStack.Com1Freq * 1000).Normalize25KhzFrequency();
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom1AudioSelection)
                                {
                                    mRadioStackState.Com1ReceiveEnabled = wrapper.XplaneData.RadioStack.Com1AudioSelection;
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom1Volume)
                                {
                                    float val = wrapper.XplaneData.RadioStack.Com1Volume;
                                    if (val > 1.0f) val = 1.0f;
                                    if (val < 0.0f) val = 0.0f;
                                    RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(1, val));
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom2Freq)
                                {
                                    mRadioStackState.Com2ActiveFrequency = ((uint)wrapper.XplaneData.RadioStack.Com2Freq * 1000).Normalize25KhzFrequency();
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom2AudioSelection)
                                {
                                    mRadioStackState.Com2ReceiveEnabled = wrapper.XplaneData.RadioStack.Com2AudioSelection;
                                }
                                if (wrapper.XplaneData.RadioStack.HasCom2Volume)
                                {
                                    float val = wrapper.XplaneData.RadioStack.Com2Volume;
                                    if (val > 1.0f) val = 1.0f;
                                    if (val < 0.0f) val = 0.0f;
                                    RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(2, val));
                                }
                                if (wrapper.XplaneData.RadioStack.HasAvionicsPowerOn)
                                {
                                    mRadioStackState.AvionicsPowerOn = wrapper.XplaneData.RadioStack.AvionicsPowerOn;
                                }
                                if (wrapper.XplaneData.RadioStack.HasTransponderCode)
                                {
                                    mRadioStackState.TransponderCode = (ushort)wrapper.XplaneData.RadioStack.TransponderCode;
                                }
                                if (wrapper.XplaneData.RadioStack.HasTransponderMode)
                                {
                                    mRadioStackState.SquawkingModeC = wrapper.XplaneData.RadioStack.TransponderMode >= 2;
                                }
                                if (wrapper.XplaneData.RadioStack.HasTransponderIdent)
                                {
                                    mRadioStackState.SquawkingIdent = wrapper.XplaneData.RadioStack.TransponderIdent;
                                }
                                RadioStackStateChanged?.Invoke(this, new RadioStackStateChangedEventArgs(mRadioStackState));
                                UpdateTunedFrequencies();
                            }

                            // user aircraft data
                            if (wrapper.XplaneData.UserAircraftData != null)
                            {
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityLatitude)
                                {
                                    mUserAircraftData.LatitudeVelocity = wrapper.XplaneData.UserAircraftData.VelocityLatitude;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityAltitude)
                                {
                                    mUserAircraftData.AltitudeVelocity = wrapper.XplaneData.UserAircraftData.VelocityAltitude;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityLongitude)
                                {
                                    mUserAircraftData.LongitudeVelocity = wrapper.XplaneData.UserAircraftData.VelocityLongitude;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityPitch)
                                {
                                    mUserAircraftData.PitchVelocity = wrapper.XplaneData.UserAircraftData.VelocityPitch;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityHeading)
                                {
                                    mUserAircraftData.HeadingVelocity = wrapper.XplaneData.UserAircraftData.VelocityHeading;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasVelocityBank)
                                {
                                    mUserAircraftData.BankVelocity = wrapper.XplaneData.UserAircraftData.VelocityBank;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasLatitude)
                                {
                                    mUserAircraftData.Latitude = wrapper.XplaneData.UserAircraftData.Latitude;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasLongitude)
                                {
                                    mUserAircraftData.Longitude = wrapper.XplaneData.UserAircraftData.Longitude;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasAltitudeMsl)
                                {
                                    mUserAircraftData.AltitudeMslM = wrapper.XplaneData.UserAircraftData.AltitudeMsl;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasAltitudeAgl)
                                {
                                    mUserAircraftData.AltitudeAglM = wrapper.XplaneData.UserAircraftData.AltitudeAgl;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasYaw)
                                {
                                    mUserAircraftData.Heading = wrapper.XplaneData.UserAircraftData.Yaw;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasPitch)
                                {
                                    mUserAircraftData.Pitch = wrapper.XplaneData.UserAircraftData.Pitch;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasRoll)
                                {
                                    mUserAircraftData.Bank = wrapper.XplaneData.UserAircraftData.Roll;
                                }
                                if (wrapper.XplaneData.UserAircraftData.HasGroundSpeed)
                                {
                                    mUserAircraftData.SpeedGround = wrapper.XplaneData.UserAircraftData.GroundSpeed;
                                }
                                UserAircraftDataUpdated?.Invoke(this, new UserAircraftDataUpdatedEventArgs(mUserAircraftData));
                            }

                            // user aircraft config
                            if (wrapper.XplaneData.UserAircraftConfig != null)
                            {
                                if (wrapper.XplaneData.UserAircraftConfig.HasBeaconLightsOn)
                                {
                                    mUserAircraftConfigData.BeaconOn = wrapper.XplaneData.UserAircraftConfig.BeaconLightsOn;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasLandingLightsOn)
                                {
                                    mUserAircraftConfigData.LandingLightsOn = wrapper.XplaneData.UserAircraftConfig.LandingLightsOn;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasNavLightsOn)
                                {
                                    mUserAircraftConfigData.NavLightOn = wrapper.XplaneData.UserAircraftConfig.NavLightsOn;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasStrobeLightsOn)
                                {
                                    mUserAircraftConfigData.StrobesOn = wrapper.XplaneData.UserAircraftConfig.StrobeLightsOn;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasTaxiLightsOn)
                                {
                                    mUserAircraftConfigData.TaxiLightsOn = wrapper.XplaneData.UserAircraftConfig.TaxiLightsOn;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasFlaps)
                                {
                                    mUserAircraftConfigData.FlapsRatio = wrapper.XplaneData.UserAircraftConfig.Flaps;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasSpeedBrakes)
                                {
                                    mUserAircraftConfigData.SpoilersRatio = wrapper.XplaneData.UserAircraftConfig.SpeedBrakes;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasGearDown)
                                {
                                    mUserAircraftConfigData.GearDown = wrapper.XplaneData.UserAircraftConfig.GearDown;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasOnGround)
                                {
                                    mUserAircraftConfigData.OnGround = wrapper.XplaneData.UserAircraftConfig.OnGround;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasEngineCount)
                                {
                                    mUserAircraftConfigData.EngineCount = wrapper.XplaneData.UserAircraftConfig.EngineCount;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasEngine1Running)
                                {
                                    mUserAircraftConfigData.Engine1Running = wrapper.XplaneData.UserAircraftConfig.Engine1Running;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasEngine2Running)
                                {
                                    mUserAircraftConfigData.Engine2Running = wrapper.XplaneData.UserAircraftConfig.Engine2Running;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasEngine3Running)
                                {
                                    mUserAircraftConfigData.Engine3Running = wrapper.XplaneData.UserAircraftConfig.Engine3Running;
                                }
                                if (wrapper.XplaneData.UserAircraftConfig.HasEngine4Running)
                                {
                                    mUserAircraftConfigData.Engine4Running = wrapper.XplaneData.UserAircraftConfig.Engine4Running;
                                }
                                UserAircraftConfigDataUpdated?.Invoke(this, new UserAircraftConfigDataUpdatedEventArgs(mUserAircraftConfigData));
                            }

                            if (wrapper.XplaneData.HasReplayMode && wrapper.XplaneData.ReplayMode)
                            {
                                ReplayModeDetected?.Invoke(this, EventArgs.Empty);
                            }

                            if (wrapper.XplaneData.HasSimPaused)
                            {
                                if (wrapper.XplaneData.SimPaused && wrapper.XplaneData.SimPaused != mIsPaused)
                                {
                                    mNetworkManager.SendEmptyFastPositionPacket();
                                }
                                mIsPaused = wrapper.XplaneData.SimPaused;
                            }
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
            SendProtobufArray(msg);
        }

        private void RequestCslValidation()
        {
            var msg = new Wrapper
            {
                CslValidation = new CslValidation()
            };
            SendProtobufArray(msg);
        }

        public void SendProtobufArray(Wrapper msg)
        {
            if (msg.ToByteArray().Length > 0)
            {
                mMessageQueue.Enqueue(msg.ToByteArray());
            }
        }

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
            SendProtobufArray(msg);
        }

        public void EnableTransponderModeC(bool enabled)
        {
            var msg = new Wrapper
            {
                SetTransponder = new SetTransponder()
            };
            msg.SetTransponder.ModeC = enabled;
            SendProtobufArray(msg);
        }

        public void TriggerTransponderIdent()
        {
            var msg = new Wrapper
            {
                SetTransponder = new SetTransponder()
            };
            msg.SetTransponder.Ident = true;
            SendProtobufArray(msg);
        }

        public void SetRadioFrequency(int radio, uint freq)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.Frequency = (int)freq;
            SendProtobufArray(msg);
        }

        public void SetRadioTransmit(int radio)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.TransmitEnabled = true;
            SendProtobufArray(msg);
        }

        public void SetRadioReceive(int radio, bool enabled)
        {
            var msg = new Wrapper
            {
                SetRadiostack = new SetRadioStack()
            };
            msg.SetRadiostack.Radio = radio;
            msg.SetRadiostack.ReceiveEnabled = enabled;
            SendProtobufArray(msg);
        }

        public void PlaneConfigChanged(AirplaneConfig config)
        {
            var msg = new Wrapper
            {
                AirplaneConfig = config
            };
            SendProtobufArray(msg);
        }

        public void NetworkConnected(string callsign)
        {
            var msg = new Wrapper
            {
                NetworkConnected = new Protobuf.NetworkConnected()
            };
            msg.NetworkConnected.Callsign = callsign;
            SendProtobufArray(msg);
        }

        public void NetworkDisconnected()
        {
            var msg = new Wrapper
            {
                NetworkDisconnected = new Protobuf.NetworkDisconnected()
            };
            SendProtobufArray(msg);
        }

        public void PlaneConfigChanged(Aircraft aircraft)
        {
            var cfg = new AirplaneConfig
            {
                Callsign = aircraft.Callsign,
                IsFullConfig = aircraft.Configuration.IsFullData.Value,
                EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning,
                OnGround = aircraft.Configuration.OnGround.Value,
                Flaps = (float)(aircraft.Configuration.FlapsPercent.Value / 100.0f),
                GearDown = aircraft.Configuration.GearDown.Value,
                Lights = new AirplaneConfig.Types.AirplaneConfigLights
                {
                    BeaconLightsOn = aircraft.Configuration.Lights.BeaconOn.Value,
                    LandingLightsOn = aircraft.Configuration.Lights.LandingOn.Value,
                    NavLightsOn = aircraft.Configuration.Lights.NavOn.Value,
                    StrobeLightsOn = aircraft.Configuration.Lights.StrobesOn.Value,
                    TaxiLightsOn = aircraft.Configuration.Lights.TaxiOn.Value
                }
            };
            var msg = new Wrapper
            {
                AirplaneConfig = cfg
            };
            SendProtobufArray(msg);
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
            SendProtobufArray(msg);
        }

        public void DeleteAircraft(Aircraft aircraft)
        {
            var msg = new Wrapper
            {
                DeletePlane = new DeletePlane()
            };
            msg.DeletePlane.Callsign = aircraft.Callsign;
            SendProtobufArray(msg);
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
            SendProtobufArray(msg);
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
            SendProtobufArray(msg);
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
            SendProtobufArray(msg);
        }
    }
}