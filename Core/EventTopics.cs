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
        public const string NotificationPosted = "NotificationPosted";
        public const string ChatSessionStarted = "ChatSessionStarted";
        public const string PlayNotificationSound = "PlaySoundRequested";
        public const string SessionStarted = "SessionStarted";
        public const string SessionEnded = "SessionEnded";
        public const string MainFormShown = "MainFormShown";
        public const string ConnectButtonStateChanged = "EnableConnectButton";
        public const string ClientConfigChanged = "ClientConfigChanged";

        // X-Plane
        public const string TransponderModeChanged = "TransponderModeChanged";
        public const string TransponderIdentStateChanged = "TransponderIdentStateChanged";
        public const string SimConnectionStateChanged = "SimConnectionStateChanged";
        public const string SimulatorMessageSent = "SimulatorMessageSent";

        // Audio for Vatsim
        public const string PushToTalkStateChanged = "PushToTalkStateChanged";
        public const string HFAliasChanged = "HFAliasChanged";
        public const string ComRadioTransmittingChanged = "ComRadioTransmittingChanged";
        public const string RadioReceiveStateChanged = "ComRadioReceivingChanged";
        public const string RadioStackStateChanged = "RadioStackStateChanged";
        public const string OverrideRadioStackState = "OverrideComStatusChanged";
        public const string RadioVolumeChanged = "RadioVolumeChanged";

        // Network Events
        public const string NetworkConnected = "NetworkConnected";
        public const string NetworkDisconnected = "NetworkDisconnected";
        public const string DeletePilotReceived = "DeletePilotReceived";
        public const string LegacyPlaneInfoReceived = "LegacyPlaneInfoReceived";
        public const string CapabilitiesResponseReceived = "CapabilitiesResponseReceived";
        public const string CapabilitiesRequestReceived = "CapabilitiesRequestReceived";
        public const string MetarRequested = "MetarRequested";
        public const string WallopSent = "WallopRequestSent";
        public const string RadioMessageSent = "RadioMessageSent";
        public const string NetworkServerListUpdated = "NetworkServerListUpdated";
        public const string ServerListDownloadFailed = "ServerListDownloadFailed";
        public const string ServerMessageReceived = "ServerMessageReceived";
        public const string BroadcastMessageReceived = "BroadcastMessageReceived";
        public const string RealNameReceived = "RealNameReceived";
        public const string MetarReceived = "MetarReceived";
        public const string SelcalAlertReceived = "SelcalAlertReceived";
        public const string RadioMessageReceived = "RadioMessageReceived";
        public const string PrivateMessageReceived = "PrivateMessageReceived";
        public const string FlightPlanReceived = "FlightPlanReceived";
        public const string RemoteFlightPlanReceived = "RemoteFlightPlanReceived";
        public const string FetchFlightPlan = "FetchFlightPlan";
        public const string SendFlightPlan = "SendFlightPlan";
        public const string RealNameRequested = "RealNameRequested";
        public const string PrivateMessageSent = "PrivateMessageSent";
        public const string IsValidAtcReceived = "IsValidAtcReceived";
        public const string AtisLinesReceived = "AtisLinesReceived";
        public const string AtisEndReceived = "AtisEndReceived";
        public const string AcarsRequestSent = "AcarsRequestSent";
        public const string AcarsResponseReceived = "AcarsResponseReceived";
        public const string SocketMessageReceived = "SocketMessageReceived";
        public const string ControllerAdded = "ControllerAdded";
        public const string ControllerDeleted = "ControllerDeleted";
        public const string ControllerFrequencyChanged = "ControllerFrequencyChanged";
        public const string ControllerUpdateReceived = "ControllerUpdateReceived";
        public const string ControllerInfoReceived = "ControllerInfoReceived";

        // Multiplayer
        public const string PlaneInfoReceived = "AircraftInfoReceived";
        public const string PilotPositionReceived = "PilotPositionReceived";
        public const string AircraftConfigurationInfoReceived = "AircraftConfigurationInfoReceived";
        public const string UserAircraftDataChanged = "UserAircraftDataChanged";
        public const string ValidateCslPaths = "ValidateCslPaths";
    }
}
