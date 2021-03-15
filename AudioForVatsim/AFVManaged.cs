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
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot.AudioForVatsim
{
    public class AFVManaged : EventBus, IAFVManaged
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.ComRadioTransmittingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioTransmittingChanged;

        [EventPublication(EventTopics.ComRadioReceivingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioReceivingChanged;

        private readonly IAppConfig mConfig;
        private readonly INetworkManager mNetworkManager;
        private bool mPttActive = false;
        private Timer mUpdateTransceiversTimer;
        private Timer mSetClientPositionTimer;
        private RadioStackState mRadioStackState;
        private UserAircraftData mUserAircraftData;
        private AFVBindings.EventCallback mEventCalllback;
        private AFVBindings.AfvStationCallback mStationCallback;
        private List<Station> mAliasStations = new List<Station>();
        private Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();

        public AFVManaged(IEventBroker eventBroker, IAppConfig config, INetworkManager networkManager) : base(eventBroker)
        {
            mConfig = config;
            mNetworkManager = networkManager;

            mUpdateTransceiversTimer = new Timer { Interval = 20000 };
            mUpdateTransceiversTimer.Elapsed += UpdateTransceiversTimer_Elapsed;

            mSetClientPositionTimer = new Timer { Interval = 5000 };
            mSetClientPositionTimer.Elapsed += SetClientPositionTimer_Elapsed;

            AFVBindings.Initialize(mConfig.AfvResourcePath, 2, "xPilot");
            SetupAudioDevices();

            mStationCallback = new AFVBindings.AfvStationCallback(StationCallbackHandler);
            mEventCalllback = new AFVBindings.EventCallback(AFVEventHandler);
            AFVBindings.RaiseClientEvent(mEventCalllback);
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnPublisher))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            if (!string.IsNullOrEmpty(mConfig.VatsimId) && !string.IsNullOrEmpty(mConfig.VatsimPasswordDecrypted))
            {
                ConfigureAudioDevices();

                AFVBindings.SetCallsign(e.ConnectInfo.Callsign);
                AFVBindings.SetCredentials(mConfig.VatsimId, mConfig.VatsimPasswordDecrypted);
                AFVBindings.AfvConnect();

                mUpdateTransceiversTimer.Start();
                mSetClientPositionTimer.Start();
            }
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnPublisher))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            AFVBindings.AfvDisconnect();
            AFVBindings.StopAudio();

            mAliasStations.Clear();
            mUpdateTransceiversTimer.Stop();
            mSetClientPositionTimer.Stop();

            ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(0, false));
            ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, false));
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (!mUserAircraftData.Equals(e.UserAircraftData))
            {
                mUserAircraftData = e.UserAircraftData;
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnPublisher))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            if (!mRadioStackState.Equals(e.RadioStackState))
            {
                mRadioStackState = e.RadioStackState;
                if (AFVBindings.IsAPIConnected())
                {
                    if (mRadioStackState.Com1TransmitEnabled)
                    {
                        AFVBindings.SetTxRadio(0);
                    }
                    else if (mRadioStackState.Com2TransmitEnabled)
                    {
                        AFVBindings.SetTxRadio(1);
                    }
                    UpdateTransceivers();
                }
            }
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnPublisher))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChangedEventArgs e)
        {
            if (!mNetworkManager.IsConnected)
            {
                return;
            }

            mPttActive = e.Pressed;
            if (AFVBindings.IsAPIConnected())
            {
                if (mRadioStackState.Com1TransmitEnabled)
                {
                    ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(0, mPttActive));
                }
                if (mRadioStackState.Com2TransmitEnabled)
                {
                    ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, mPttActive));
                }
                AFVBindings.SetPtt(mPttActive);
            }
        }

        [EventSubscription(EventTopics.ControllerAdded, typeof(OnUserInterfaceAsync))]
        public void OnControllerAdded(object sender, ControllerEventArgs e)
        {
            mControllers[e.Controller.Callsign] = e.Controller;
            UpdateTransceivers();
        }

        [EventSubscription(EventTopics.ControllerDeleted, typeof(OnPublisher))]
        public void OnControllerDeleted(object sender, ControllerEventArgs e)
        {
            if (mControllers.Remove(e.Controller.Callsign))
            {
                UpdateTransceivers();
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

        private void SetClientPositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (AFVBindings.IsAPIConnected())
            {
                AFVBindings.SetClientPosition(mUserAircraftData.Latitude, mUserAircraftData.Longitude, mUserAircraftData.AltitudeMslM, mUserAircraftData.AltitudeAglM);
            }
        }

        private void SetupAudioDevices()
        {
            if (!AFVBindings.IsClientInitialized())
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error initializing AFV. No audio devices could be initialized."));
                return;
            }

            mConfig.AudioDrivers.Clear();
            mConfig.InputDevices.Clear();
            mConfig.OutputDevices.Clear();

            AFVBindings.GetAudioApis(mConfig.AudioDrivers.SafeAdd);
            if (!string.IsNullOrEmpty(mConfig.AudioDriver))
            {
                SetAudioDriver(mConfig.AudioDriver);
            }
        }

        public void SetAudioDriver(string driver)
        {
            AFVBindings.SetAudioApi(driver);

            mConfig.OutputDevices.Clear();
            mConfig.InputDevices.Clear();

            AFVBindings.GetOutputDevices(mConfig.OutputDevices.SafeAdd);
            AFVBindings.GetInputDevices(mConfig.InputDevices.SafeAdd);
        }

        public void ConfigureAudioDevices()
        {
            AFVBindings.StopAudio();
            AFVBindings.SetAudioDevice();
            if (!string.IsNullOrEmpty(mConfig.ListenDeviceName))
            {
                AFVBindings.SetAudioOutputDevice(mConfig.ListenDeviceName);
            }
            if (!string.IsNullOrEmpty(mConfig.InputDeviceName))
            {
                AFVBindings.SetAudioOutputDevice(mConfig.InputDeviceName);
            }
            AFVBindings.SetEnableHfSquelch(mConfig.EnableHfSquelch);
            AFVBindings.SetEnableOutputEffects(!mConfig.DisableAudioEffects);
            AFVBindings.StartAudio();
            UpdateRadioGains();
        }

        public void UpdateRadioGains()
        {
            AFVBindings.SetRadioGain(0, mConfig.Com1Volume / 100.0f);
            AFVBindings.SetRadioGain(1, mConfig.Com2Volume / 100.0f);
        }

        private void AFVEventHandler(AFVEvents evt, int data)
        {
            switch (evt)
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
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Voice server channel error: " + data));
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
                case AFVEvents.RxStarted:
                    ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(data, true));
                    break;
                case AFVEvents.RxStopped:
                    ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(data, false));
                    break;
                case AFVEvents.AudioError:
                    break;
            }
        }

        private void GetStationAliases()
        {
            AFVBindings.GetStationAliases(mStationCallback);
        }

        private void UpdateTransceiversTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTransceivers();
        }

        private void UpdateTransceivers()
        {
            CreateTransceiver(0, mRadioStackState.Com1ReceiveEnabled ? mRadioStackState.Com1ActiveFrequency : 0);
            CreateTransceiver(1, mRadioStackState.Com2ReceiveEnabled ? mRadioStackState.Com2ActiveFrequency : 0);
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
            }

            AFVBindings.SetRadioState(id, (int)frequencyHertz);
        }

        public void DisbleRadioEffects(bool disabled)
        {
            AFVBindings.SetEnableOutputEffects(!disabled);
        }

        public void EnableHFSquelch(bool enable)
        {
            AFVBindings.SetEnableHfSquelch(enable);
        }
    }
}