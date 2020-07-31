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
using GeoVR.Client;
using System.Collections.Generic;
using Vatsim.Fsd.Connector;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Network;

namespace XPilot.PilotClient.Config
{
    public interface IAppConfig
    {
        List<NetworkServerInfo> CachedServers { get; set; }
        string VatsimId { get; set; }
        string VatsimPassword { get; set; }
        string ServerName { get; set; }
        List<ConnectInfo> RecentConnectionInfo { get; set; }
        FlightPlan LastFlightPlan { get; set; }
        PTTConfiguration PTTConfiguration { get; set; }
        WindowProperties ClientWindowProperties { get; set; }
        ToggleDisplayConfiguration ToggleDisplayConfiguration { get; set; }
        string InputDeviceName { get; set; }
        string OutputDeviceName { get; set; }
        bool DisableAudioEffects { get; set; }
        float Com1Volume { get; set; }
        float Com2Volume { get; set; }
        float InputVolumeDb { get; set; }
        EqualizerPresets VhfEqualizer { get; set; }
        string Name { get; set; }
        string HomeAirport { get; set; }
        void SaveConfig();
        void LoadConfig(string path);
        string AppPath { get; set; }
        bool FlashTaskbarPrivateMessage { get; set; }
        bool FlashTaskbarRadioMessage { get; set; }
        bool FlashTaskbarSelCal { get; set; }
        bool FlashTaskbarDisconnect { get; set; }
        bool AutoSquawkModeC { get; set; }
        bool KeepClientWindowVisible { get; set; }
        bool CheckForUpdates { get; set; }
        string NameWithAirport { get; }
        bool ConfigurationRequired { get; }
        int TcpPort { get; set; }
        bool PlayGenericSelCalAlert { get; set; }
        bool PlayRadioMessageAlert { get; set; }
        UpdateChannel UpdateChannel { get; set; }
        bool SquawkingModeC { get; set; }
        string XplanePath { get; set; }
        bool VolumeKnobsControlVolume { get; set; }
    }
}