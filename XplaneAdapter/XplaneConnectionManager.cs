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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Controllers;
using XPlaneConnector;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace XPilot.PilotClient.XplaneAdapter
{
    public class XplaneConnectionManager : EventBus, IXplaneConnectionManager
    {
        const int MIN_PLUGIN_VERSION = 1336; // 1.3.10 = 1310

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.UserAircraftDataUpdated)]
        public event EventHandler<UserAircraftDataUpdatedEventArgs> UserAircraftDataUpdated;

        [EventPublication(EventTopics.SimConnectionStateChanged)]
        public event EventHandler<ClientEventArgs<bool>> SimConnectionStateChanged;

        [EventPublication(EventTopics.RadioStackStateChanged)]
        public event EventHandler<RadioStackStateChangedEventArgs> RadioStackStateChanged;

        [EventPublication(EventTopics.TransponderModeChanged)]
        public event EventHandler<ClientEventArgs<bool>> TransponderModeChanged;

        [EventPublication(EventTopics.SquawkingIdentChanged)]
        public event EventHandler<SquawkingIdentChangedEventArgs> SquawkingIdentChanged;

        [EventPublication(EventTopics.PlaySoundRequested)]
        public event EventHandler<PlaySoundEventArgs> PlaySoundRequested;

        [EventPublication(EventTopics.PushToTalkStateChanged)]
        public event EventHandler<PushToTalkStateChangedEventArgs> PushToTalkStateChanged;

        [EventPublication(EventTopics.RequestControllerAtis)]
        public event EventHandler<RequestControllerAtisEventArgs> RequestControllerAtisSent;

        [EventPublication(EventTopics.SocketMessageReceived)]
        public event EventHandler<ClientEventArgs<string>> RaiseSocketMessageReceived;

        [EventPublication(EventTopics.RadioTextMessage)]
        public event EventHandler<SimulatorMessageEventArgs> SimulatorTextMessageSent;

        [EventPublication(EventTopics.EnableConnectButton)]
        public event EventHandler<ClientEventArgs<bool>> EnableConnectButton;

        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSentEventArgs> PrivateMessageSent;

        [EventPublication(EventTopics.Com1Volume)]
        public event EventHandler<RadioVolumeChangedEventArgs> Com1VolumeChanged;

        [EventPublication(EventTopics.Com2Volume)]
        public event EventHandler<RadioVolumeChangedEventArgs> Com2VolumeChanged;

        private readonly IFsdManger mFsdManager;
        private readonly IAppConfig mConfig;

        private readonly NetMQPoller mPoller;
        private readonly NetMQQueue<string> mMessageQueue;
        private readonly DealerSocket mDealerSocket;
        private readonly List<DealerSocket> mVisualDealerSockets;
        private readonly StreamWriter mRawDataStream;

        private readonly Timer mConnectionTimer;
        private readonly Timer mRetryConnectionTimer;
        private readonly Timer mGetXplaneDataTimer;
        private readonly Timer mWhosOnlineListRefresh;

        private readonly List<DateTime> mConnectionHeartbeats = new List<DateTime>();
        private readonly List<Controller> mControllers = new List<Controller>();

        private readonly XPlaneConnector.XPlaneConnector mXplaneConnector;
        private readonly UserAircraftData mUserAircraftData;
        private readonly UserAircraftRadioStack mRadioStackState;

        private bool mInvalidPluginVersionShown = false;
        private bool mXplaneConnected = false;
        private bool mReceivedInitialHandshake = false;
        private bool mDisconnectMessageSent = false;
        private bool mValidCsl = false;
        private string mSimulatorIP = "127.0.0.1";

        public XplaneConnectionManager(IEventBroker broker, IAppConfig config, IFsdManger fsdManager) : base(broker)
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
                        visualDealerSocket.Connect("tcp://" + mIP + ":" + mConfig.TcpPort);
                        if (mVisualDealerSockets == null) mVisualDealerSockets = new List<DealerSocket>();
                        mVisualDealerSockets.Add(visualDealerSocket);
                    }
                    catch (AddressAlreadyInUseException)
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Plugin port already in use. Please choose a different TCP port."));
                    }
                }
            }

            if (!string.IsNullOrEmpty(mConfig.SimClientIP))
            {
                mSimulatorIP = mConfig.SimClientIP;
            }

            mMessageQueue = new NetMQQueue<string>();
            mDealerSocket = new DealerSocket();
            mDealerSocket.Options.TcpKeepalive = true;
            mDealerSocket.Options.Identity = Encoding.UTF8.GetBytes("CLIENT");
            mDealerSocket.ReceiveReady += DealerSocket_ReceiveReady;
            try
            {
                mDealerSocket.Connect("tcp://" + mSimulatorIP + ":" + mConfig.TcpPort);
            }
            catch (AddressAlreadyInUseException)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Plugin port already in use. Please choose a different TCP port."));
            }
            mPoller = new NetMQPoller { mDealerSocket, mMessageQueue };
            if (!mPoller.IsRunning)
            {
                mPoller.RunAsync();
            }

            mMessageQueue.ReceiveReady += (s, e) =>
            {
                if (mMessageQueue.TryDequeue(out string msg, TimeSpan.FromMilliseconds(100)))
                {
                    if (mDealerSocket != null)
                    {
                        mDealerSocket.SendFrame(msg);
                    }
                    if (mVisualDealerSockets != null && mVisualDealerSockets.Count > 0)
                    {
                        foreach (DealerSocket socket in mVisualDealerSockets)
                        {
                            socket.SendFrame(msg);
                        }

                    }
                }
            };

            mXplaneConnector = new XPlaneConnector.XPlaneConnector(mSimulatorIP);
            mUserAircraftData = new UserAircraftData();
            mRadioStackState = new UserAircraftRadioStack();

            mGetXplaneDataTimer = new Timer
            {
                Interval = 10
            };
            mGetXplaneDataTimer.Tick += GetXplaneDataTimer_Tick;
            mGetXplaneDataTimer.Start();

            mConnectionTimer = new Timer
            {
                Interval = 50
            };
            mConnectionTimer.Tick += ConnectionTimer_Tick;
            mConnectionTimer.Start();

            mRetryConnectionTimer = new Timer
            {
                Interval = 1000
            };
            mRetryConnectionTimer.Tick += RetryConnectionTimer_Tick;
            mRetryConnectionTimer.Start();

            mWhosOnlineListRefresh = new Timer
            {
                Interval = 5000
            };
            mWhosOnlineListRefresh.Tick += WhosOnlineListRefresh_Tick;

            SetupSubscribers();

            if (!Directory.Exists(Path.Combine(mConfig.AppPath, "PluginLogs")))
            {
                Directory.CreateDirectory(Path.Combine(mConfig.AppPath, "PluginLogs"));
            }

            var directory = new DirectoryInfo(Path.Combine(mConfig.AppPath, "PluginLogs"));
            var query = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in query.OrderByDescending(file => file.CreationTime).Skip(10))
            {
                file.Delete();
            }

            mRawDataStream = new StreamWriter(Path.Combine(mConfig.AppPath, string.Format($"PluginLogs/PluginLog-{DateTime.UtcNow:yyyyMMddHHmmss}.log")), false);
        }

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();
            if (!string.IsNullOrEmpty(command))
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<XplaneConnect>(command);
                    dynamic data = json.Data;
                    switch (json.Type)
                    {
                        case XplaneConnect.MessageType.PluginHash:
                            string hash = data.Hash;
                            if (!string.IsNullOrEmpty(hash))
                            {
                                mFsdManager.ClientProperties.PluginHash = hash;
                            }
                            break;
                        case XplaneConnect.MessageType.RequestAtis:
                            string callsign = data.Callsign;
                            if (!string.IsNullOrEmpty(callsign))
                            {
                                RequestControllerAtisSent?.Invoke(this, new RequestControllerAtisEventArgs(callsign));
                            }
                            break;
                        case XplaneConnect.MessageType.PluginVersion:
                            mReceivedInitialHandshake = true;
                            int pluginVersion = (int)data.Version;
                            if (pluginVersion != MIN_PLUGIN_VERSION)
                            {
                                EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
                                if (!mInvalidPluginVersionShown)
                                {
                                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: Incorrect xPilot Plugin Version. You are using an out of date xPilot plugin. Please close X-Plane and reinstall xPilot."));
                                    PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                                    mInvalidPluginVersionShown = true;
                                }
                            }
                            break;
                        case XplaneConnect.MessageType.SocketMessage:
                            {
                                string msg = data.Message;
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    RaiseSocketMessageReceived?.Invoke(this, new ClientEventArgs<string>(msg));
                                }
                            }
                            break;
                        case XplaneConnect.MessageType.PrivateMessageSent:
                            {
                                string msg = data.Message;
                                string to = data.To;
                                if (mFsdManager.IsConnected)
                                {
                                    PrivateMessageSent?.Invoke(this, new PrivateMessageSentEventArgs(mFsdManager.OurCallsign, to, msg));
                                }
                            }
                            break;
                        case XplaneConnect.MessageType.ValidateCslPaths:
                            mValidCsl = (bool)data.Result;
                            if (data.Result == false)
                            {
                                EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error: No valid CSL paths are configured or enabled, or you have no CSL models installed. Please verify the CSL configuration in X-Plane (Plugins > xPilot > Preferences). If you need assistance configuring your CSL paths, see the \"CSL Model Configuration\" section in the xPilot Documentation (http://docs.xpilot-project.org). Restart X-Plane and xPilot after you have properly configured your CSL models."));
                                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                            }
                            break;
                        case XplaneConnect.MessageType.ForceDisconnect:
                            string reason = data.Reason;
                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, reason));
                            PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                            mFsdManager.Disconnect(new DisconnectInfo
                            {
                                Type = DisconnectType.Intentional
                            });
                            break;
                    }
                }
                catch (JsonException ex)
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error deserializing JSON object: " + ex.Message));
                }
            }
        }

        public void SendMessage(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                mRawDataStream.Write($"[{ DateTime.UtcNow:HH:mm:ss.fff}] >>   { msg }\n");
                mRawDataStream.Flush();
                mMessageQueue.Enqueue(msg);
            }
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

        private void RetryConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (!mXplaneConnected)
            {
                mXplaneConnector.Start();
            }
        }

        private void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (mConnectionHeartbeats.Count > 0)
            {
                TimeSpan span = DateTime.Now - mConnectionHeartbeats.Last();
                if (span.TotalSeconds < 15)
                {
                    if (!mReceivedInitialHandshake)
                    {
                        SimConnectionStateChanged(this, new ClientEventArgs<bool>(false));
                        EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
                        SendMessage(new XplaneConnect
                        {
                            Type = XplaneConnect.MessageType.PluginVersion,
                            Timestamp = DateTime.Now
                        }.ToJSON());
                        SendMessage(new XplaneConnect
                        {
                            Type = XplaneConnect.MessageType.PluginHash,
                            Timestamp = DateTime.Now
                        }.ToJSON());
                    }
                    else
                    {
                        mXplaneConnected = true;
                        SimConnectionStateChanged(this, new ClientEventArgs<bool>(true));
                        if (mValidCsl && !mInvalidPluginVersionShown)
                        {
                            EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(true));
                        }
                    }
                    mXplaneConnected = true;
                    mConnectionHeartbeats.RemoveAt(0);
                }
                else
                {
                    mXplaneConnected = false;
                    mReceivedInitialHandshake = false;
                    SimConnectionStateChanged(this, new ClientEventArgs<bool>(false));
                    EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
                }
            }
            else
            {
                mXplaneConnected = false;
                mReceivedInitialHandshake = false;
                SimConnectionStateChanged(this, new ClientEventArgs<bool>(false));
                EnableConnectButton?.Invoke(this, new ClientEventArgs<bool>(false));
            }
        }

        private void SetupSubscribers()
        {
            // com audio selection
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsAudioComSelection, 5, (e, v) => { mRadioStackState.ComAudioSelection = (uint)e.Value; });

            // com1 frequency
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom1FrequencyHz, 5, (e, v) => { mRadioStackState.Com1ActiveFreq = ((uint)e.Value * 10000).Normalize25KhzFrequency(); });
            // com1 standby frequency
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom1StandbyFrequencyHz, 5, (e, v) => { mRadioStackState.Com1StandbyFreq = ((uint)e.Value * 10000).Normalize25KhzFrequency(); });
            // com1 power
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom1Power, 5, (e, v) => { mRadioStackState.Com1Power = Convert.ToBoolean(e.Value); });
            // com1 listening
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsAudioSelectionCom1, 5, (e, v) => { mRadioStackState.Com1Listening = Convert.ToBoolean(e.Value); });
            // com1 volume
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsAudioVolumeCom1, 5, (e, v) =>
            {
                if (e.Value > 1.0f) e.Value = 1.0f;
                if (e.Value < 0.0f) e.Value = 0.0f;
                Com1VolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(1, e.Value));
            });

            // com2 frequency
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom2FrequencyHz, 5, (e, v) => { mRadioStackState.Com2ActiveFreq = ((uint)e.Value * 10000).Normalize25KhzFrequency(); });
            // com2 standby frequency
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom2StandbyFrequencyHz, 5, (e, v) => { mRadioStackState.Com2StandbyFreq = ((uint)e.Value * 10000).Normalize25KhzFrequency(); });
            // com2 power
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsCom2Power, 5, (e, v) => { mRadioStackState.Com2Power = Convert.ToBoolean(e.Value); });
            // com2 listening
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsAudioSelectionCom2, 5, (e, v) => { mRadioStackState.Com2Listening = Convert.ToBoolean(e.Value); });
            // com2 volume
            mXplaneConnector.Subscribe(DataRefs.Cockpit2RadiosActuatorsAudioVolumeCom2, 5, (e, v) =>
            {
                if (e.Value > 1.0f) e.Value = 1.0f;
                if (e.Value < 0.0f) e.Value = 0.0f;
                Com2VolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(2, e.Value));
            });

            // avionics power
            mXplaneConnector.Subscribe(DataRefs.Cockpit2SwitchesAvionicsPowerOn, 5, (e, v) => { mRadioStackState.AvionicsPower = Convert.ToBoolean(e.Value); });

            // latitude
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionLatitude, 5, (e, v) => { mUserAircraftData.Latitude = e.Value; });

            // longitude
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionLongitude, 5, (e, v) => { mUserAircraftData.Longitude = e.Value; });

            // true altitude msl
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionElevation, 5, (e, v) => { mUserAircraftData.AltitudeMsl = e.Value; });

            // pressure altitude
            mXplaneConnector.Subscribe(DataRefs.Cockpit2GaugesIndicatorsAltitudeFtPilot, 5, (e, v) => { mUserAircraftData.PressureAltitude = e.Value; });

            // heading
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionPsi, 5, (e, v) => { mUserAircraftData.Heading = e.Value; });

            // ground speed
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionGroundspeed, 5, (e, v) => { mUserAircraftData.GroundSpeed = e.Value * 1.94384; });

            // pitch
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionTheta, 5, (e, v) => { mUserAircraftData.Pitch = e.Value; });

            // bank
            mXplaneConnector.Subscribe(DataRefs.FlightmodelPositionPhi, 5, (e, v) => { mUserAircraftData.Bank = e.Value; });

            // transponder code
            mXplaneConnector.Subscribe(DataRefs.CockpitRadiosTransponderCode, 5, (e, v) => { mUserAircraftData.TransponderCode = Convert.ToInt32(e.Value); });

            // transponder mode
            mXplaneConnector.Subscribe(DataRefs.CockpitRadiosTransponderMode, 5, (e, v) => { mUserAircraftData.TransponderMode = Convert.ToInt32(e.Value); });

            // transponder ident
            mXplaneConnector.Subscribe(DataRefs.CockpitRadiosTransponderId, 5, (e, v) => { mUserAircraftData.TransponderIdent = Convert.ToBoolean(e.Value); SquawkingIdentChanged?.Invoke(this, new SquawkingIdentChangedEventArgs(Convert.ToBoolean(e.Value))); });

            // beacon light
            mXplaneConnector.Subscribe(DataRefs.CockpitElectricalBeaconLightsOn, 5, (e, v) => { mUserAircraftData.BeaconLightsOn = Convert.ToBoolean(e.Value); });

            // landing lights
            mXplaneConnector.Subscribe(DataRefs.CockpitElectricalLandingLightsOn, 5, (e, v) => { mUserAircraftData.LandingLightsOn = Convert.ToBoolean(e.Value); });

            // taxi lights
            mXplaneConnector.Subscribe(DataRefs.CockpitElectricalTaxiLightOn, 5, (e, v) => { mUserAircraftData.TaxiLightsOn = Convert.ToBoolean(e.Value); });

            // nav lights
            mXplaneConnector.Subscribe(DataRefs.CockpitElectricalNavLightsOn, 5, (e, v) => { mUserAircraftData.NavLightsOn = Convert.ToBoolean(e.Value); });

            // strobe lights
            mXplaneConnector.Subscribe(DataRefs.CockpitElectricalStrobeLightsOn, 5, (e, v) => { mUserAircraftData.StrobeLightsOn = Convert.ToBoolean(e.Value); });

            // flaps
            mXplaneConnector.Subscribe(DataRefs.FlightmodelControlsFlaprat, 5, (e, v) => { mUserAircraftData.FlapsPercentage = e.Value; });

            // gear
            mXplaneConnector.Subscribe(DataRefs.CockpitSwitchesGearHandleStatus, 5, (e, v) => { mUserAircraftData.GearDown = Convert.ToBoolean(e.Value); });

            // speed brakes
            mXplaneConnector.Subscribe(DataRefs.Cockpit2ControlsSpeedbrakeRatio, 5, (e, v) => { mUserAircraftData.SpeedBrakeDeployed = e.Value > 0; });

            // replay mode
            mXplaneConnector.Subscribe(DataRefs.OperationPrefsReplayMode, 5, (e, v) => { mUserAircraftData.ReplayModeEnabled = Convert.ToBoolean(e.Value); });

            // engine count
            var engCount = new DataRefElement
            {
                DataRef = "sim/aircraft/engine/acf_num_engines",
                Frequency = 5
            };
            mXplaneConnector.Subscribe(engCount, 5, (e, v) => { mUserAircraftData.EngineCount = Convert.ToInt32(e.Value); });

            // engine1 running
            var engn0 = new DataRefElement
            {
                DataRef = "sim/flightmodel/engine/ENGN_running[0]",
                Frequency = 5
            };

            // engine2 running
            var engn1 = new DataRefElement
            {
                DataRef = "sim/flightmodel/engine/ENGN_running[1]",
                Frequency = 5
            };

            // engine3 running
            var engn2 = new DataRefElement
            {
                DataRef = "sim/flightmodel/engine/ENGN_running[2]",
                Frequency = 5
            };

            // engine1 running
            var engn3 = new DataRefElement
            {
                DataRef = "sim/flightmodel/engine/ENGN_running[3]",
                Frequency = 5
            };

            mXplaneConnector.Subscribe(engn0, 5, (e, v) => { mUserAircraftData.Engine1Running = Convert.ToBoolean(e.Value); });
            mXplaneConnector.Subscribe(engn1, 5, (e, v) => { mUserAircraftData.Engine2Running = Convert.ToBoolean(e.Value); });
            mXplaneConnector.Subscribe(engn2, 5, (e, v) => { mUserAircraftData.Engine3Running = Convert.ToBoolean(e.Value); });
            mXplaneConnector.Subscribe(engn3, 5, (e, v) => { mUserAircraftData.Engine4Running = Convert.ToBoolean(e.Value); });

            // on the ground
            mXplaneConnector.Subscribe(DataRefs.FlightmodelFailuresOngroundAny, 5, (e, v) => { mUserAircraftData.OnGround = Convert.ToBoolean(e.Value); });

            // ptt command
            var ptt = new DataRefElement
            {
                DataRef = "xpilot/ptt",
                Frequency = 15
            };
            mXplaneConnector.Subscribe(ptt, 15, (e, v) =>
            {
                PushToTalkStateChanged?.Invoke(this, new PushToTalkStateChangedEventArgs(Convert.ToBoolean(e.Value)));
            });

            mXplaneConnector.Start();
        }

        private void GetXplaneDataTimer_Tick(object sender, EventArgs e)
        {
            if (mXplaneConnector.LastReceive == DateTime.MinValue)
            {
                return;
            }

            if (mUserAircraftData.TransponderMode >= 2)
            {
                TransponderModeChanged?.Invoke(this, new ClientEventArgs<bool>(true));
            }
            else
            {
                TransponderModeChanged?.Invoke(this, new ClientEventArgs<bool>(false));
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

            UserAircraftDataUpdated?.Invoke(this, new UserAircraftDataUpdatedEventArgs(mUserAircraftData));
            RadioStackStateChanged?.Invoke(this, new RadioStackStateChangedEventArgs(radio));

            if (mUserAircraftData.Latitude != 0.0 && mUserAircraftData.Longitude != 0.0)
            {
                mConnectionHeartbeats.Add(mXplaneConnector.LastReceive);
            }
        }

        [EventSubscription(EventTopics.RadioVolumeChanged, typeof(OnUserInterfaceAsync))]
        public void OnVolumeLevelsChanged(object sender, RadioVolumeChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 1:
                    mXplaneConnector.SetDataRefValue(new DataRefElement
                    {
                        DataRef = "sim/cockpit2/radios/actuators/audio_volume_com1"
                    }, e.Volume);
                    break;
                case 2:
                    mXplaneConnector.SetDataRefValue(new DataRefElement
                    {
                        DataRef = "sim/cockpit2/radios/actuators/audio_volume_com2"
                    }, e.Volume);
                    break;
            }
        }

        [EventSubscription(EventTopics.OverrideComStatus, typeof(OnUserInterfaceAsync))]
        public void OnOverrideComStatus(object sender, OverrideComStatusEventArgs e)
        {
            mRadioStackState.Com1ForceRx = e.Com1Rx;
            mRadioStackState.Com1ForceTx = e.Com1Tx;
            mRadioStackState.Com2ForceRx = e.Com2Rx;
            mRadioStackState.Com2ForceTx = e.Com2Tx;
        }

        [EventSubscription(EventTopics.SendXplaneCommand, typeof(OnUserInterfaceAsync))]
        public void OnXplaneCommand(object sender, ClientEventArgs<XPlaneCommand> e)
        {
            mXplaneConnector.SendCommand(e.Value);
        }

        [EventSubscription(EventTopics.SetXplaneDataRefValue, typeof(OnUserInterfaceAsync))]
        public void OnSetXplaneDataRefValue(object sender, DataRefEventArgs e)
        {
            mXplaneConnector.SetDataRefValue(e.DataRef, e.Value);
        }

        [EventSubscription(EventTopics.XPlaneEventPosted, typeof(OnUserInterfaceAsync))]
        public void OnXplaneEventPosted(object sender, ClientEventArgs<string> e)
        {
            SendMessage(e.Value);
        }

        [EventSubscription(EventTopics.RadioTextMessage, typeof(OnUserInterfaceAsync))]
        public void OnRadioTextMessage(object sender, SimulatorMessageEventArgs e)
        {
            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.RadioMessage,
                Timestamp = DateTime.Now,
                Data = new XplaneTextMessage
                {
                    Message = e.Text,
                    Red = e.Red,
                    Green = e.Green,
                    Blue = e.Blue,
                    Direct = e.PriorityMessage
                }
            }.ToJSON());
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChangedEventArgs e)
        {
            if (mFsdManager.IsConnected)
            {
                var ptt = new DataRefElement
                {
                    DataRef = "xpilot/ptt"
                };
                mXplaneConnector.SetDataRefValue(ptt, e.Down ? 1.0f : 0.0f);
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
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
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
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
                SimulatorTextMessageSent?.Invoke(this, new SimulatorMessageEventArgs("Disconnected from network.", priorityMessage: true));
                mDisconnectMessageSent = true;
            }

            mWhosOnlineListRefresh.Stop();
            mControllers.Clear();
        }

        [EventSubscription(EventTopics.ControllerAdded, typeof(OnUserInterfaceAsync))]
        public void OnControllerAdded(object sender, ControllerEventArgs e)
        {
            if (!mControllers.Contains(e.Controller))
            {
                mControllers.Add(e.Controller);
            }
        }

        [EventSubscription(EventTopics.ControllerDeleted, typeof(OnUserInterfaceAsync))]
        public void OnControllerDeleted(object sender, ControllerEventArgs e)
        {
            if (mControllers.Contains(e.Controller))
            {
                mControllers.Remove(e.Controller);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void OnControllerFrequencyChanged(object sender, ControllerEventArgs e)
        {
            var controller = mControllers.FirstOrDefault(o => o.Callsign == e.Controller.Callsign);
            if (controller != null)
            {
                controller.Frequency = e.Controller.Frequency;
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            var controller = mControllers.FirstOrDefault(o => o.Callsign == e.From);
            if (controller != null)
            {
                controller.RealName = e.Data;
            }
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From != "SERVER")
            {
                dynamic data = new ExpandoObject();
                data.From = e.From.ToUpper();
                data.Message = e.Data;

                SendMessage(new XplaneConnect
                {
                    Type = XplaneConnect.MessageType.PrivateMessageReceived,
                    Timestamp = DateTime.Now,
                    Data = data
                }.ToJSON());
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
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
            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.PluginVersion,
                Timestamp = DateTime.Now
            }.ToJSON());
            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.PluginHash,
                Timestamp = DateTime.Now
            }.ToJSON());
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
            SendMessage(new XplaneConnect
            {
                Type = XplaneConnect.MessageType.ValidateCslPaths,
                Timestamp = DateTime.Now
            }.ToJSON());
        }

        [EventSubscription(EventTopics.MicrophoneInputLevelChanged, typeof(OnUserInterfaceAsync))]
        public void MicrophoneInputLevelChanged(object sender, ClientEventArgs<float> e)
        {
            var dr = new DataRefElement
            {
                DataRef = "xpilot/audio/vu"
            };
            mXplaneConnector.SetDataRefValue(dr, e.Value);
        }
    }
}