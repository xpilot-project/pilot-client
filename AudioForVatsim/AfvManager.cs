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
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using GeoVR.Client;
using GeoVR.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient.AudioForVatsim
{
    public class AfvManager : EventBus, IAfvManager
    {
        [EventPublication(EventTopics.ComRadioTransmittingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioTransmittingChanged;

        [EventPublication(EventTopics.ComRadioReceivingChanged)]
        public event EventHandler<ComRadioTxRxChangedEventArgs> ComRadioReceivingChanged;

        [EventPublication(EventTopics.MicrophoneInputLevelChanged)]
        public event EventHandler<ClientEventArgs<float>> MicrophoneInputLevelChanged;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.Com1FrequencyAliasChanged)]
        public event EventHandler<ComFrequencyAliasChangedEventArgs> Com1FrequencyAliasChanged;

        [EventPublication(EventTopics.Com2FrequencyAliasChanged)]
        public event EventHandler<ComFrequencyAliasChangedEventArgs> Com2FrequencyAliasChanged;

        [EventPublication(EventTopics.PlaySoundRequested)]
        public event EventHandler<PlaySoundEventArgs> PlaySoundRequested;

        [EventPublication(EventTopics.SetXplaneDataRefValue)]
        public event EventHandler<DataRefEventArgs> SetXplaneDataRefValue;

        [EventPublication(EventTopics.VoiceServerConnectionLost)]
        public event EventHandler<EventArgs> RaiseVoiceServerConnectionLost;

        private const string VOICE_SERVER_URL = "https://voice1.vatsim.uk";
        private bool mPttActive = false;
        private string mInputDeviceName;
        private string mOutputDeviceName;
        private UserAircraftData mUserAircraftData;
        private UserAircraftRadioStack mRadioStackState;
        private readonly UserClient mAfvUserClient;
        private readonly Timer mUpdateTransceiverTimer;
        private readonly IAppConfig mConfig;
        private readonly IFsdManger mFsdManager;
        private IEnumerable<StationDto> mAliasedStations = new List<StationDto>();
        private readonly Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();
        private readonly List<TransceiverDto> mTransceivers = new List<TransceiverDto>();

        public AfvManager(IEventBroker broker, IAppConfig config, IFsdManger fsdManager) : base(broker)
        {
            mConfig = config;
            mFsdManager = fsdManager;

            mUpdateTransceiverTimer = new Timer
            {
                Interval = 20000
            };
            mUpdateTransceiverTimer.Tick += UpdateTransceiverTimer_Tick;

            mAfvUserClient = new UserClient(VOICE_SERVER_URL, ReceivingCallsignsChanged);
            mAfvUserClient.Connected += AfvUserClient_Connected;
            mAfvUserClient.Disconnected += AfvUserClient_Disconnected;
            mAfvUserClient.InputVolumeStream += AfvUserClient_InputVolumeStream;
        }

        private void AfvUserClient_Connected(object sender, ConnectedEventArgs e)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Connected to voice server."));
        }

        private void AfvUserClient_Disconnected(object sender, DisconnectedEventArgs e)
        {
            if (e.Reason != "Intentional")
            {
                RaiseVoiceServerConnectionLost?.Invoke(this, EventArgs.Empty);
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                if (e.AutoReconnect)
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, $"Disconnected from voice server: { e.Reason }. Attempting to auto-reconnect..."));
                }
                else
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, $"Disconnected from voice server: { e.Reason }"));
                }
            }
        }

        private void AfvUserClient_InputVolumeStream(object sender, InputVolumeStreamEventArgs e)
        {
            MicrophoneInputLevelChanged?.Invoke(this, new ClientEventArgs<float>(e.PeakVU));
        }

        private void ReceivingCallsignsChanged(object sender, TransceiverReceivingCallsignsChangedEventArgs e)
        {
            switch (e.TransceiverID)
            {
                case 1:
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new XPlaneConnector.DataRefElement
                    {
                        DataRef = "xpilot/audio/com1_rx"
                    }, e.ReceivingCallsigns.Count > 0 ? 1 : 0));
                    ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, e.ReceivingCallsigns.Count > 0));
                    break;
                case 2:
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new XPlaneConnector.DataRefElement
                    {
                        DataRef = "xpilot/audio/com2_rx"
                    }, e.ReceivingCallsigns.Count > 0 ? 1 : 0));
                    ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, e.ReceivingCallsigns.Count > 0));
                    break;
            }
        }

        private void PrepareTransceivers()
        {
            foreach (TransceiverDto dto in mTransceivers)
            {
                dto.LatDeg = mUserAircraftData.Latitude;
                dto.LonDeg = mUserAircraftData.Longitude;
                dto.HeightAglM = mUserAircraftData.AltitudeMsl;
                dto.HeightMslM = mUserAircraftData.AltitudeMsl;
            }
            UpdateRadioTransceivers();
        }

        private void UpdateRadioTransceivers()
        {
            if (mAfvUserClient.IsConnected)
            {
                mAfvUserClient.UpdateTransceivers(mTransceivers);
            }
        }

        private void UpdateTransceiverLists()
        {
            mTransceivers.Clear();
            if (mRadioStackState.IsCom1Receiving)
            {
                mTransceivers.Add(CreateTransceiver(1, mRadioStackState.Com1ActiveFreq));
            }
            if (mRadioStackState.IsCom2Receiving)
            {
                mTransceivers.Add(CreateTransceiver(2, mRadioStackState.Com2ActiveFreq));
            }
        }

        private TransceiverDto CreateTransceiver(ushort id, uint frequency)
        {
            uint frequencyHertz = frequency.Normalize25KhzFrequency();

            var aliasedStation = mAliasedStations.FirstOrDefault(station =>
                station.FrequencyAlias.HasValue
                && station.Frequency.HasValue
                && mControllers.Values.Any(c =>
                    (c.NormalizedFrequency == frequency.MatchNetworkFormat())
                    && c.Callsign.FuzzyMatches(station.Name)
                    && c.Frequency.FsdFrequencyToHertz() == station.FrequencyAlias.Value)
            );

            if (aliasedStation != null)
            {
                frequencyHertz = aliasedStation.Frequency.Value;
                UpdateAliasTooltips(id, aliasedStation.Frequency.Value);
            }
            else
            {
                UpdateAliasTooltips(id, 0);
            }

            return new TransceiverDto()
            {
                ID = id,
                Frequency = frequencyHertz,
                LatDeg = mUserAircraftData.Latitude,
                LonDeg = mUserAircraftData.Longitude,
                HeightAglM = mUserAircraftData.AltitudeMsl,
                HeightMslM = mUserAircraftData.AltitudeMsl
            };
        }

        private void UpdateAliasTooltips(ushort id, uint alias)
        {
            if (alias > 0)
            {
                switch (id)
                {
                    case 1:
                        Com1FrequencyAliasChanged?.Invoke(this, new ComFrequencyAliasChangedEventArgs(1, alias));
                        break;
                    case 2:
                        Com2FrequencyAliasChanged?.Invoke(this, new ComFrequencyAliasChangedEventArgs(2, alias));
                        break;
                }
            }
            else
            {
                switch (id)
                {
                    case 1:
                        Com1FrequencyAliasChanged?.Invoke(this, new ComFrequencyAliasChangedEventArgs(0, 0));
                        break;
                    case 2:
                        Com2FrequencyAliasChanged?.Invoke(this, new ComFrequencyAliasChangedEventArgs(0, 0));
                        break;
                }
            }
        }

        private void SetAicraftRadioStatus()
        {
            if (mRadioStackState.IsCom1Transmitting)
            {
                mAfvUserClient.TransmittingTransceivers(1);
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, mPttActive));
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, false));
            }
            else if (mRadioStackState.IsCom2Transmitting)
            {
                mAfvUserClient.TransmittingTransceivers(2);
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, false));
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, mPttActive));
            }
            else
            {
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, false));
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, false));
            }
        }

        private void StopClientConnection()
        {
            try
            {
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, false));
                ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, false));
                ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, false));
                ComRadioReceivingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, false));
                if (mAfvUserClient.Started)
                {
                    mAfvUserClient.Stop();
                }
                mUpdateTransceiverTimer.Stop();
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error stopping voice server client: " + ex.Message));
            }
        }

        private void StartClientConnection()
        {
            try
            {
                if (!mAfvUserClient.Started)
                {
                    if (!ClientAudioUtilities.IsInputDevicePresent())
                    {
                        mOutputDeviceName = mConfig.OutputDeviceName;
                        mAfvUserClient.Com1Volume = mConfig.Com1Volume;
                        mAfvUserClient.Com2Volume = mConfig.Com2Volume;
                        mAfvUserClient.Start(mOutputDeviceName, new List<ushort> { 1, 2 });
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "No audio input device was detected. xPilot will run in receive only mode."));
                    }
                    else
                    {
                        mInputDeviceName = mConfig.InputDeviceName;
                        mOutputDeviceName = mConfig.OutputDeviceName;
                        mAfvUserClient.InputVolumeDb = mConfig.InputVolumeDb / 4;
                        mAfvUserClient.Com1Volume = mConfig.Com1Volume;
                        mAfvUserClient.Com2Volume = mConfig.Com2Volume;
                        mAfvUserClient.Start(mInputDeviceName, mOutputDeviceName, new List<ushort> { 1, 2 });
                    }
                    mAfvUserClient.BypassEffects = mConfig.DisableAudioEffects;
                }
                mUpdateTransceiverTimer.Start();
                PrepareTransceivers();
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error starting voice server client: " + ex.Message));
            }
        }

        private void UpdateTransceiverTimer_Tick(object sender, EventArgs e)
        {
            PrepareTransceivers();
        }

        public void SetAudioEffectsDisabled(bool disabled)
        {
            mAfvUserClient.BypassEffects = disabled;
        }

        public void UpdateVolumes()
        {
            mAfvUserClient.Com1Volume = mConfig.Com1Volume;
            mAfvUserClient.Com2Volume = mConfig.Com2Volume;
            mAfvUserClient.InputVolumeDb = mConfig.InputVolumeDb;
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            if (!mAfvUserClient.Started)
            {
                StartClientConnection();
            }
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterfaceAsync))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            StopClientConnection();
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public async void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            try
            {
                if (!mAfvUserClient.IsConnected && !e.ConnectInfo.TowerViewMode)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

                    await mAfvUserClient.Connect(mConfig.VatsimId, mConfig.VatsimPassword, mFsdManager.OurCallsign, "xPilot " + fvi.FileVersion);
                    mAliasedStations = await mAfvUserClient.ApiServerConnection.GetAllAliasedStations();
                    PrepareTransceivers();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Connect failed (Forbidden - )")
                {
                    NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error connecting to voice server. Please check your login credentials and try again. If your VATSIM account is new, please allow up to 24-hours for your account to completely synchronize."));
                }
                else
                {
                    NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error connecting to voice server: { ex.Message }"));
                }
            }
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public async void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            if (mAfvUserClient.IsConnected)
            {
                try
                {
                    await mAfvUserClient.Disconnect("Intentional");
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Disconnected from the voice server."));
                }
                catch (Exception ex)
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error disconnecting from voice server: { ex.Message }"));
                }
            }
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (mUserAircraftData == null || !mUserAircraftData.Equals(e.UserAircraftData))
            {
                mUserAircraftData = e.UserAircraftData;
            }
        }

        [EventSubscription(EventTopics.PushToTalkStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnPushToTalkStateChanged(object sender, PushToTalkStateChangedEventArgs e)
        {
            mPttActive = e.Down;
            if (mAfvUserClient.IsConnected && mAfvUserClient.Started && ClientAudioUtilities.IsInputDevicePresent())
            {
                if (mRadioStackState.IsCom1Transmitting)
                {
                    ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(1, mPttActive));
                }
                if (mRadioStackState.IsCom2Transmitting)
                {
                    ComRadioTransmittingChanged?.Invoke(this, new ComRadioTxRxChangedEventArgs(2, mPttActive));
                }
                mAfvUserClient.PTT(mPttActive);
            }
        }

        [EventSubscription(EventTopics.DeleteControllerReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeleteControllerReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                mControllers.Remove(controller.Callsign);
                UpdateTransceiverLists();
                UpdateRadioTransceivers();
            }
        }

        [EventSubscription(EventTopics.ControllerUpdateReceived, typeof(OnUserInterfaceAsync))]
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

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void OnControllerFrequencyChanged(object sender, ControllerEventArgs e)
        {
            if (mControllers.ContainsKey(e.Controller.Callsign))
            {
                mControllers[e.Controller.Callsign] = e.Controller;
                UpdateTransceiverLists();
                UpdateRadioTransceivers();
            }
        }

        [EventSubscription(EventTopics.RestartAfvUserClient, typeof(OnUserInterfaceAsync))]
        public void OnRestartAfvUserClient(object sender, EventArgs e)
        {
            StopClientConnection();
            StartClientConnection();
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            if (mRadioStackState == null || !mRadioStackState.Equals(e.RadioStackState))
            {
                mRadioStackState = e.RadioStackState;
                UpdateTransceiverLists();
                UpdateRadioTransceivers();
                SetAicraftRadioStatus();
            }
        }
    }
}
