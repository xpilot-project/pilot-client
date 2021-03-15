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
namespace Vatsim.Xpilot.Core
{
    public class EventTopics
    {
        public const string NotificationPosted = "NotificationPosted";
        public const string ChatSessionStarted = "ChatSessionStarted";
        public const string PlayNotificationSound = "PlaySoundRequested";
        public const string SessionStarted = "SessionStarted";
        public const string SessionEnded = "SessionEnded";
        public const string MainFormShown = "MainFormShown";
        public const string SettingsModified = "SettingsModified";

        // X-Plane
        public const string SimulatorConnected = "SimulatorConnected";
        public const string SimulatorDisconnected = "SimulatorDisconnected";
        public const string ConnectButtonEnabled = "ConnectButtonEnabled";
        public const string ConnectButtonDisabled = "ConnectButtonDisabled";
        public const string SimulatorMessageSent = "SimulatorMessageSent";

        // Audio for Vatsim
        public const string PushToTalkStateChanged = "PushToTalkStateChanged";
        public const string HFAliasChanged = "HFAliasChanged";
        public const string ComRadioTransmittingChanged = "ComRadioTransmittingChanged";
        public const string ComRadioReceivingChanged = "ComRadioReceivingChanged";
        public const string RadioStackStateChanged = "RadioStackStateChanged";
        public const string OverrideRadioStackState = "OverrideComStatusChanged";
        public const string RadioVolumeChanged = "RadioVolumeChanged";

        // Network Events
        public const string NetworkConnectionInitiated = "NetworkConnectionInitiated";
        public const string NetworkConnectionFailed = "NetworkConnectionFailed";
        public const string NetworkConnected = "NetworkConnected";
        public const string NetworkDisconnectionInitiated = "NetworkDisconnectionInitiated";
        public const string NetworkDisconnected = "NetworkDisconnected";
        public const string NetworkServerListUpdated = "NetworkServerListUpdated";
        public const string FlightPlanReceived = "FlightPlanReceived";
        public const string RadioMessageReceived = "RadioMessageReceived";
        public const string RadioMessageSent = "RadioMessageSent";
        public const string BroadcastMessageReceived = "BroadcastMessageReceived";
        public const string PrivateMessageReceived = "PrivateMessageReceived";
        public const string PrivateMessageSent = "PrivateMessageSent";
        public const string ServerMessageReceived = "ServerMessageReceived";
        public const string DeleteControllerReceived = "DeleteControllerReceived";
        public const string DeletePilotReceived = "DeletePilotReceived";
        public const string AircraftUpdateReceived = "AircraftUpdateReceived";
        public const string FastPositionUpdateReceived = "FastPositionUpdateReceived";
        public const string ControllerUpdateReceived = "ControlerUpdateReceived";
        public const string CapabilitiesRequestReceived = "CapabilitiesRequestReceived";
        public const string AircraftConfigurationInfoReceived = "AircraftConfigurationInfoReceived";
        public const string RealNameReceived = "RealNameReceived";
        public const string RealNameRequested = "RealNameRequested";
        public const string IsValidATCReceived = "IsValidATCReceived";
        public const string AtisLineReceived = "AtisLineReceived";
        public const string AtisEndReceived = "AtisEndReceived";
        public const string CapabilitiesResponseReceived = "CapabilitiesResponseReceived";
        public const string AircraftInfoReceived = "AircraftInfoReceived";
        public const string MetarReceived = "MetarReceived";
        public const string SelcalAlertReceived = "SelcalAlertReceived";
        public const string ControllerAdded = "ControllerAdded";
        public const string ControllerDeleted = "ControllerDeleted";
        public const string ControllerFrequencyChanged = "ControllerFrequencyChanged";
        public const string ControllerSupportsNewInfoChanged = "ControllerSupportsNewInfoChanged";
        public const string ControllerLocationChanged = "ControllerLocationChanged";
        public const string RequestedAtisReceived = "RequestedAtisReceived";
        public const string WallopSent = "WallopSent";
        public const string AircraftAddedToSimulator = "AircraftAddedToSimulator";
        public const string UserAircraftDataUpdated = "UserAircraftDataChanged";
        public const string UserAircraftConfigDataUpdated = "UserAircraftConfigDataUpdated";
    }
}
