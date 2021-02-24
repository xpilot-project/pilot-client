/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2021 Justin Shannon
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
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient.AudioForVatsim
{
    public class AFVManaged : IAFVManaged
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.ComRadioTransmittingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioTransmittingChanged;

        [EventPublication(EventTopics.ComRadioReceivingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioReceivingChanged;

        [EventPublication(EventTopics.ComFrequencyAliasChanged)]
        public event EventHandler<ComFrequencyAliasChangedEventArgs> RaiseComFrequencyAliasChanged;

        private readonly IEventBroker mEventBroker;
        private readonly IAppConfig mConfig;
        private readonly ILog mLog;
        private bool mPttActive = false;
        private Timer mRxTimer;
        private Timer mUpdateTransceiversTimer;
        private UserAircraftRadioStack mRadioStackState;
        private AFVBindings.EventCallback mEventCalllback;
        private AFVBindings.AfvStationCallback mStationCallback;
        private List<Station> mAliasStations = new List<Station>();
        private Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();

        public AFVManaged(IEventBroker eventBroker, IAppConfig config, ILog log)
        {
            mEventBroker = eventBroker;
            mEventBroker.Register(this);
            mLog = log;
            mConfig = config;

            AFVBindings.initialize(mConfig.AppPath, 2, "xPilot");
            SetupAudioDevices();

            mRxTimer = new Timer
            {
                Interval = 100
            };
            mRxTimer.Elapsed += RxTimer_Elapsed;

            mUpdateTransceiversTimer = new Timer
            {
                Interval = 5000
            };
            mUpdateTransceiversTimer.Elapsed += UpdateTransceiversTimer_Elapsed;

            mStationCallback = new AFVBindings.AfvStationCallback(StationCallbackHandler);
            mEventCalllback = new AFVBindings.EventCallback(ClientEventHandler);
            AFVBindings.RaiseClientEvent(mEventCalllback);

            mLog.Info("AFV-Native Initialized");
        }

        private void UpdateTransceiversTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTransceivers();
        }

        private void RxTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(0, AFVBindings.IsCom1Rx()));
            ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, AFVBindings.IsCom2Rx()));
        }

        public void GetStationAliases()
        {
            AFVBindings.getStationAliases(mStationCallback);
        }

        private void StationCallbackHandler(string id, string callsign, uint frequency, uint frequencyAlias)
        {
            mAliasStations.Add(new Station
            {
                ID = id,
                Callsign = callsign,
                Frequency = frequency,
                FrequencyAlias = frequencyAlias
            });
        }

        private void SetupAudioDevices()
        {
            if (!AFVBindings.isClientInitialized())
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Could not setup audio devices. AFV-Native is not initialized."));
                return;
            }

            mConfig.AudioApis.Clear();
            mConfig.InputDevices.Clear();
            mConfig.OutputDevices.Clear();

            AFVBindings.getAudioApis(mConfig.AudioApis.SafeAdd);

            if (!string.IsNullOrEmpty(mConfig.AudioApi))
            {
                AFVBindings.setAudioApi(mConfig.AudioApi);
                AFVBindings.getOutputDevices(mConfig.OutputDevices.SafeAdd);
                AFVBindings.getInputDevices(mConfig.InputDevices.SafeAdd);
            }
        }

        public void SetAudioApi(string api)
        {
            AFVBindings.setAudioApi(api);

            mConfig.OutputDevices.Clear();
            mConfig.InputDevices.Clear();

            AFVBindings.getOutputDevices(mConfig.OutputDevices.SafeAdd);
            AFVBindings.getInputDevices(mConfig.InputDevices.SafeAdd);
        }

        private void ConfigureAudioDevices()
        {
            AFVBindings.setAudioDevice();
            if (!string.IsNullOrEmpty(mConfig.OutputDeviceName))
            {
                AFVBindings.setAudioOutputDevice(mConfig.OutputDeviceName);
            }
            if (!string.IsNullOrEmpty(mConfig.InputDeviceName))
            {
                AFVBindings.setAudioInputDevice(mConfig.InputDeviceName);
            }
            AFVBindings.stopAudio();
            AFVBindings.startAudio();
        }

        private void UpdateTransceivers()
        {
            if (mRadioStackState != null)
            {
                CreateTransceiver(0, mRadioStackState.IsCom1Receiving ? mRadioStackState.Com1ActiveFreq : 0);
                CreateTransceiver(1, mRadioStackState.IsCom2Receiving ? mRadioStackState.Com2ActiveFreq : 0);
            }
        }

        private void CreateTransceiver(ushort id, uint frequency)
        {
            uint frequencyHertz = frequency.Normalize25KhzFrequency();

            var aliasedStation = mAliasStations.FirstOrDefault(station =>
                station.FrequencyAlias.HasValue
                && station.Frequency.HasValue
                && mControllers.Values.Any(c =>
                    (c.NormalizedFrequency == frequency.MatchNetworkFormat())
                    && c.Callsign.FuzzyMatches(station.Callsign)
                    && c.Frequency.FsdFrequencyToHertz() == station.FrequencyAlias.Value));

            if (aliasedStation != null)
            {
                frequencyHertz = aliasedStation.Frequency.Value;
                UpdateAliasTooltips(id, aliasedStation.Frequency.Value);
            }
            else
            {
                UpdateAliasTooltips(id, 0);
            }

            AFVBindings.setRadioState(id, (int)frequencyHertz);

            switch (id)
            {
                case 0:
                    AFVBindings.setRadioGain(id, mConfig.Com1Volume / 100.0f);
                    break;
                case 1:
                    AFVBindings.setRadioGain(id, mConfig.Com2Volume / 100.0f);
                    break;
            }
        }

        private void UpdateAliasTooltips(ushort radio, uint alias)
        {
            RaiseComFrequencyAliasChanged?.Invoke(this, new ComFrequencyAliasChangedEventArgs(radio, alias));
        }

        private void ClientEventHandler(AFVEvents afvEvent, int data)
        {
            switch (afvEvent)
            {
                case AFVEvents.APIServerError:
                    {
                        APISessionError error = (APISessionError)data;
                        switch (error)
                        {
                            case APISessionError.BadPassword:
                            case APISessionError.RejectedCredentials:
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error connecting to voice server. Please check your VATSIM credentials and try again."));
                                break;
                            case APISessionError.ConnectionError:
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error initiating connection to the voice server."));
                                break;
                            default:
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Voice server error: " + error));
                                break;
                        }
                    }
                    break;
                case AFVEvents.VoiceServerChannelError:
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Voice server channel error: " + data));
                    }
                    break;
                case AFVEvents.VoiceServerError:
                    {
                        VoiceSessionError error = (VoiceSessionError)data;
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Voice server error: " + error));
                    }
                    break;
                case AFVEvents.StationAliasesUpdated:
                    GetStationAliases();
                    break;
                case AFVEvents.VoiceServerConnected:
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Connected to voice server"));
                    break;
                case AFVEvents.VoiceServerDisconnected:
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Disconnected from voice server"));
                    break;
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnPublisher))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            if (!string.IsNullOrEmpty(mConfig.VatsimId) && !string.IsNullOrEmpty(mConfig.VatsimPasswordDecrypted))
            {
                ConfigureAudioDevices();

                AFVBindings.setCallsign(e.ConnectInfo.Callsign);
                AFVBindings.setCredentials(mConfig.VatsimId, mConfig.VatsimPasswordDecrypted);
                AFVBindings.afvConnect();

                mRxTimer.Start();
                mUpdateTransceiversTimer.Start();
                mLog.Info("AFVNative connected.");
            }
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnPublisher))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            AFVBindings.afvDisconnect();
            AFVBindings.stopAudio();

            mAliasStations.Clear();
            mRxTimer.Stop();
            mUpdateTransceiversTimer.Stop();
            mLog.Info("AFVNative disconnected.");
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnPublisher))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (AFVBindings.isAPIConnected())
            {
                AFVBindings.setClientPosition(e.UserAircraftData.Latitude, e.UserAircraftData.Longitude, e.UserAircraftData.AltitudeMsl, e.UserAircraftData.AltitudeAgl);
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnPublisher))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            mRadioStackState = e.RadioStackState;
            if (AFVBindings.isAPIConnected())
            {
                if (mRadioStackState.IsCom1Transmitting)
                {
                    AFVBindings.setTxRadio(0);
                }
                else if (mRadioStackState.IsCom2Transmitting)
                {
                    AFVBindings.setTxRadio(1);
                }
                UpdateTransceivers();
            }
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnPublisher))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChangedEventArgs e)
        {
            mPttActive = e.Down;
            if (mRadioStackState != null)
            {
                if (AFVBindings.isAPIConnected())
                {
                    if (mRadioStackState.IsCom1Transmitting)
                    {
                        ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(0, mPttActive));
                    }
                    if (mRadioStackState.IsCom2Transmitting)
                    {
                        ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, mPttActive));
                    }
                    AFVBindings.setPtt(mPttActive);

                }
            }
        }

        [EventSubscription(EventTopics.DeleteControllerReceived, typeof(OnPublisher))]
        public void OnDeleteControllerReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                mControllers.Remove(controller.Callsign);
                UpdateTransceivers();
            }
        }

        [EventSubscription(EventTopics.ControllerUpdateReceived, typeof(OnPublisher))]
        public void OnControllerUpdated(object sender, ControllerUpdateReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                controller.Frequency = e.Frequency;
                controller.NormalizedFrequency = e.Frequency.Normalize25KhzFsdFrequency();
                controller.Location = e.Location;
                controller.LastUpdate = DateTime.Now;
            }
            else
            {
                Controller controller = new Controller
                {
                    Callsign = e.From,
                    Frequency = e.Frequency,
                    NormalizedFrequency = e.Frequency.Normalize25KhzFsdFrequency(),
                    Location = e.Location,
                    LastUpdate = DateTime.Now,
                    RealName = "Unknown"
                };
                mControllers.Add(controller.Callsign, controller);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnPublisher))]
        public void OnControllerFrequencyChanged(object sender, ControllerEventArgs e)
        {
            if (mControllers.ContainsKey(e.Controller.Callsign))
            {
                mControllers[e.Controller.Callsign] = e.Controller;
                UpdateTransceivers();
            }
        }
    }
}