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
    public class AFVManaged : EventBus, IAFVManaged
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPosted> RaiseNotificationPosted;

        [EventPublication(EventTopics.ComRadioTransmittingChanged)]
        public event EventHandler<RadioTxStateChanged> RaiseRadioTransmitStateChanged;

        [EventPublication(EventTopics.RadioReceiveStateChanged)]
        public event EventHandler<RadioRxStateChanged> RaiseRadioReceiveStateChanged;

        [EventPublication(EventTopics.HFAliasChanged)]
        public event EventHandler<HFAliasChanged> RaiseHFAliasChanged;

        private readonly IAppConfig mConfig;
        private bool mPttActive = false;
        private Timer mUpdateTransceiversTimer;
        private UserAircraftRadioStack mRadioStackState;
        private AFVBindings.EventCallback mEventCalllback;
        private AFVBindings.AfvStationCallback mStationCallback;
        private List<Station> mAliasStations = new List<Station>();
        private Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();

        public AFVManaged(IEventBroker eventBroker, IAppConfig config) : base(eventBroker)
        {
            mConfig = config;

            mUpdateTransceiversTimer = new Timer { Interval = 5000 };
            mUpdateTransceiversTimer.Elapsed += UpdateTransceiversTimer_Elapsed;

            AFVBindings.initialize(mConfig.AfvResourcePath, 2, "xPilot");
            SetupAudioDevices();

            mStationCallback = new AFVBindings.AfvStationCallback(StationCallbackHandler);
            mEventCalllback = new AFVBindings.EventCallback(AFVEventHandler);
            AFVBindings.RaiseClientEvent(mEventCalllback);
        }

        private void SetupAudioDevices()
        {
            if (!AFVBindings.isClientInitialized())
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Error initializing AFV. The audio devices could not be initialized."));
                return;
            }

            mConfig.AudioDrivers.Clear();
            mConfig.InputDevices.Clear();
            mConfig.OutputDevices.Clear();

            AFVBindings.getAudioApis(mConfig.AudioDrivers.SafeAdd);
            if (!string.IsNullOrEmpty(mConfig.AudioDriver))
            {
                SetAudioDriver(mConfig.AudioDriver);
            }
        }

        public void SetAudioDriver(string driver)
        {
            AFVBindings.setAudioApi(driver);

            mConfig.OutputDevices.Clear();
            mConfig.InputDevices.Clear();

            AFVBindings.getOutputDevices(mConfig.OutputDevices.SafeAdd);
            AFVBindings.getInputDevices(mConfig.InputDevices.SafeAdd);
        }

        public void ConfigureAudioDevices()
        {
            AFVBindings.stopAudio();
            AFVBindings.setAudioDevice();
            if (!string.IsNullOrEmpty(mConfig.ListenDeviceName))
            {
                AFVBindings.setAudioOutputDevice(mConfig.ListenDeviceName);
            }
            if (!string.IsNullOrEmpty(mConfig.InputDeviceName))
            {
                AFVBindings.setAudioOutputDevice(mConfig.InputDeviceName);
            }
            AFVBindings.startAudio();
        }

        public void UpdateRadioGains()
        {
            AFVBindings.setRadioGain(0, mConfig.Com1Volume / 100.0f);
            AFVBindings.setRadioGain(1, mConfig.Com2Volume / 100.0f);
        }

        private void AFVEventHandler(AFVEvents evt, int data)
        {
            Console.WriteLine(">> " + evt);
            switch (evt)
            {
                case AFVEvents.APIServerError:
                    {
                        APISessionError error = (APISessionError)data;
                        switch (error)
                        {
                            case APISessionError.BadPassword:
                            case APISessionError.RejectedCredentials:
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Error connecting to voice server. Please check your VATSIM credentials and try again."));
                                break;
                            case APISessionError.ConnectionError:
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Error initiating connection to the voice server."));
                                break;
                            default:
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Voice server error: " + error));
                                break;
                        }
                    }
                    break;
                case AFVEvents.VoiceServerChannelError:
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Voice server channel error: " + data));
                    break;
                case AFVEvents.VoiceServerError:
                    {
                        VoiceSessionError error = (VoiceSessionError)data;
                        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Voice server error: " + error));
                    }
                    break;
                case AFVEvents.StationAliasesUpdated:
                    GetStationAliases();
                    break;
                case AFVEvents.VoiceServerConnected:
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "Connected to voice server"));
                    break;
                case AFVEvents.VoiceServerDisconnected:
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "Disconnected from voice server"));
                    break;
                case AFVEvents.RxStarted:
                    RaiseRadioReceiveStateChanged?.Invoke(this, new RadioRxStateChanged(data, true));
                    break;
                case AFVEvents.RxStopped:
                    RaiseRadioReceiveStateChanged?.Invoke(this, new RadioRxStateChanged(data, false));
                    break;
                case AFVEvents.AudioError:
                    break;
            }
        }

        private void GetStationAliases()
        {
            AFVBindings.getStationAliases(mStationCallback);
        }

        private void UpdateTransceiversTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTransceivers();
        }

        private void UpdateTransceivers()
        {
            if (mRadioStackState != null)
            {
                CreateTransceiver(0, mRadioStackState.IsCom1Receiving ? mRadioStackState.Com1ActiveFreq : 0);
                CreateTransceiver(1, mRadioStackState.IsCom2Receiving ? mRadioStackState.Com2ActiveFreq : 0);
            }
        }

        private void StationCallbackHandler(string id, string callsign, uint frequency, uint frequencyAlias)
        {
            mAliasStations.Clear();
            mAliasStations.Add(new Station
            {
                ID = id,
                Callsign = callsign,
                Frequency = frequency,
                FrequencyAlias = frequencyAlias
            });
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
            RaiseHFAliasChanged?.Invoke(this, new HFAliasChanged(radio, alias));
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnPublisher))]
        public void OnNetworkConnected(object sender, NetworkConnected e)
        {
            if (!string.IsNullOrEmpty(mConfig.VatsimId) && !string.IsNullOrEmpty(mConfig.VatsimPasswordDecrypted))
            {
                ConfigureAudioDevices();

                AFVBindings.setCallsign(e.ConnectInfo.Callsign);
                AFVBindings.setCredentials(mConfig.VatsimId, mConfig.VatsimPasswordDecrypted);
                AFVBindings.afvConnect();

                mUpdateTransceiversTimer.Start();
            }
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnPublisher))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnected e)
        {
            AFVBindings.afvDisconnect();
            AFVBindings.stopAudio();

            mAliasStations.Clear();
            mUpdateTransceiversTimer.Stop();

            RaiseRadioReceiveStateChanged?.Invoke(this, new RadioRxStateChanged(0, false));
            RaiseRadioReceiveStateChanged?.Invoke(this, new RadioRxStateChanged(1, false));
        }

        [EventSubscription(EventTopics.UserAircraftDataChanged, typeof(OnPublisher))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataChanged e)
        {
            if (AFVBindings.isAPIConnected())
            {
                AFVBindings.setClientPosition(e.AircraftData.Latitude, e.AircraftData.Longitude, e.AircraftData.AltitudeMsl, e.AircraftData.AltitudeAgl);
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnPublisher))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChanged e)
        {
            mRadioStackState = e.RadioStack;
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
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChanged e)
        {
            mPttActive = e.Pressed;
            if (mRadioStackState != null)
            {
                if (AFVBindings.isAPIConnected())
                {
                    if (mRadioStackState.IsCom1Transmitting)
                    {
                        RaiseRadioTransmitStateChanged?.Invoke(this, new RadioTxStateChanged(0, mPttActive));
                    }
                    if (mRadioStackState.IsCom2Transmitting)
                    {
                        RaiseRadioTransmitStateChanged?.Invoke(this, new RadioTxStateChanged(1, mPttActive));
                    }
                    AFVBindings.setPtt(mPttActive);

                }
            }
        }

        [EventSubscription(EventTopics.DeleteControllerReceived, typeof(OnPublisher))]
        public void OnDeleteControllerReceived(object sender, NetworkDataReceived e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                var controller = mControllers[e.From];
                mControllers.Remove(controller.Callsign);
                UpdateTransceivers();
            }
        }

        [EventSubscription(EventTopics.ControllerUpdateReceived, typeof(OnPublisher))]
        public void OnControllerDictionaryUpdateds(object sender, ControllerUpdateReceived e)
        {
            if (mControllers.ContainsKey(e.Controller.Callsign))
            {
                Controller controller = mControllers[e.Controller.Callsign];
                controller.Frequency = e.Controller.Frequency;
                controller.NormalizedFrequency = e.Controller.NormalizedFrequency;
                controller.Location = e.Controller.Location;
                controller.LastUpdate = DateTime.Now;
            }
            else
            {
                Controller controller = new Controller
                {
                    Callsign = e.Controller.Callsign,
                    Frequency = e.Controller.Frequency,
                    NormalizedFrequency = e.Controller.NormalizedFrequency,
                    Location = e.Controller.Location,
                    LastUpdate = DateTime.Now,
                    RealName = "Unknown"
                };
                mControllers.Add(controller.Callsign, controller);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnPublisher))]
        public void OnControllerFrequencyChanged(object sender, ControllerUpdateReceived e)
        {
            if (mControllers.ContainsKey(e.Controller.Callsign))
            {
                mControllers[e.Controller.Callsign] = e.Controller;
                UpdateTransceivers();
            }
        }
    }
}