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
using System.Threading.Tasks;

namespace XPilot.PilotClient
{
    public class EventTopics
    {
        // Forms
        public const string MainFormShown = "MainFormShown";
        public const string SettingsFormShown = "SettingsFormShown";
        public const string MainFormClosed = "MainFormClosed";

        // Notifications
        public const string NotificationPosted = "NotificiationPosted";

        // Network Events
        public const string NetworkConnected = "NetworkConnected";
        public const string NetworkDisconnected = "NetworkDisconnected";
        public const string DeletePilotReceived = "DeletePilotReceived";
        public const string LegacyPlaneInfoReceived = "LegacyPlaneInfoReceived";
        public const string CapabilitiesResponseReceived = "CapabilitiesResponseReceived";
        public const string CapabilitiesRequestReceived = "CapabilitiesRequestReceived";
        public const string MetarRequested = "MetarRequested";
        public const string WallopRequestSent = "WallopRequestSent";
        public const string ControllerAtisRequested = "ControllerAtisRequested";
        public const string RadioMessageSent = "RadioMessageSent";
        public const string NetworkServerListUpdated = "NetworkServerListUpdated";
        public const string ServerListDownloadFailed = "ServerListDownloadFailed";
        public const string ServerMessageReceived = "ServerMessageReceived";
        public const string BroadcastMessageReceived = "BroadcastMessageReceived";
        public const string ControllerAdded = "ControllerAdded";
        public const string RealNameReceived = "RealNameReceived";
        public const string ControllerUpdateReceived = "ControllerUpdateReceived";
        public const string DeleteControllerReceived = "DeleteControllerReceived";
        public const string ControllerFrequencyChanged = "ControllerFrequencyChanged";
        public const string MetarReceived = "MetarReceived";
        public const string SelcalAlertReceived = "SelcalAlertReceived";
        public const string RadioMessageReceived = "RadioMessageReceived";
        public const string PrivateMessageReceived = "PrivateMessageReceived";
        public const string NoFlightPlanReceived = "NoFlightPlanReceived";
        public const string FlightPlanReceived = "FlightPlanReceived";
        public const string RequestFlightPlan = "RequestFlightPlan";
        public const string FileFlightPlan = "FileFlightPlan";
        public const string RealNameRequested = "RealNameRequested";
        public const string PrivateMessageSent = "PrivateMessageSent";
        public const string IsValidAtcReceived = "IsValidAtcReceived";
        public const string ControllerLocationChanged = "ControllerLocationChagned";
        public const string ControllerSupportsNewInfoChanged = "ControllerSupportsNewInfoChanged";
        public const string ControllerInfoReceived = "ControllerInfoReceived";
        public const string AtisLinesReceived = "AtisLinesReceived";
        public const string AtisEndReceived = "AtisEndReceived";
        public const string ControllerDeleted = "ControllerDeleted";
        public const string SquawkingIdentChanged = "SquawkingIdentChanged";
        public const string RequestedAtisReceived = "RequestedAtisReceived";
        public const string RequestControllerAtis = "RequestControllerAtis";
        public const string SocketMessageReceived = "SocketMessageReceived";

        // X-Plane
        public const string XPlaneEventPosted = "XPlaneEventPosted";
        public const string TransponderModeChanged = "TransponderModeChanged";
        public const string SendXplaneCommand = "SendXplaneCommand";
        public const string SetXplaneDataRefValue = "SetXplaneDataRefValue";
        public const string SimConnectionStateChanged = "SimConnectionStateChanged";
        public const string RadioTextMessage = "RadioTextMessage";
        public const string SimulatorMessage = "SimulatorMessage";
        public const string LowFrameRateAlert = "LowFrameRateAlert";
        public const string ToggleClientDisplay = "ToggleClientDisplay";

        // Audio for Vatsim
        public const string PushToTalkStateChanged = "PushToTalkStateChanged";
        public const string Com1FrequencyAliasChanged = "Com1FrequencyAliasChanged";
        public const string Com2FrequencyAliasChanged = "Com2FrequencyAliasChanged";
        public const string ComRadioTransmittingChanged = "ComRadioTransmittingChanged";
        public const string ComRadioReceivingChanged = "ComRadioReceivingChanged";
        public const string MicrophoneInputLevelChanged = "MicrophoneInputLevelChanged";
        public const string RadioStackStateChanged = "RadioStackStateChanged";
        public const string OverrideComStatus = "OverrideComStatusChanged";
        public const string RestartAfvUserClient = "RestartAfvUserClient";
        public const string Com1Volume = "Com1Volume";
        public const string Com2Volume = "Com2Volume";
        public const string RadioVolumeChanged = "RadioVolumeChanged";

        // Settings
        public const string ClientConfigChanged = "ClientConfigChanged";
        public const string PluginPortChanged = "PluginPortChanged";

        // Multiplayer
        public const string NetworkAircraftInfoReceived = "AircraftInfoReceived";
        public const string NetworkAircraftUpdateReceived = "AircraftPositionUpdateReceived";
        public const string AircraftConfigurationInfoReceived = "AircraftConfigurationInfoReceived";
        public const string UserAircraftDataUpdated = "UserAircraftDataUpdated";
        public const string ValidateCslPaths = "ValidateCslPaths";

        // Miscellaneous
        public const string ChatSessionStarted = "ChatSessionStarted";
        public const string PlaySoundRequested = "PlaySoundRequested";
        public const string PlaySelcalRequested = "PlaySelcalRequested";
        public const string SessionStarted = "SessionStarted";
        public const string SessionEnded = "SessionEnded";
        public const string VoiceServerConnectionLost = "VoiceServerConnectionLost";
        public const string ToggleConnectButtonState = "ToggleConnectButtonState";
    }
}
