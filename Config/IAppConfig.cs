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
using System.Collections.Generic;
using Vatsim.FsdClient;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot.Config
{
    public interface IAppConfig
    {
        Dictionary<int, string> AudioDrivers { get; set; }
        Dictionary<int, string> OutputDevices { get; set; }
        Dictionary<int, string> InputDevices { get; set; }
        bool IsVoiceDisabled { get; }
        string AppPath { get; set; }
        string AfvResourcePath { get; }
        bool ConfigurationRequired { get; }
        bool SquawkingModeC { get; set; }
        List<NetworkServerInfo> CachedServers { get; set; }
        string VatsimId { get; set; }
        string VatsimPasswordDecrypted { get; set; }
        string Name { get; set; }
        string NameWithAirport { get; }
        string HomeAirport { get; set; }
        string ServerName { get; set; }
        bool AutoSquawkModeC { get; set; }
        string AudioDriver { get; set; }
        string InputDeviceName { get; set; }
        string ListenDeviceName { get; set; }
        bool DisableAudioEffects { get; set; }
        bool EnableNotificationSounds { get; set; }
        bool EnableHfSquelch { get; set; }
        int Com1Volume { get; set; }
        int Com2Volume { get; set; }
        List<ConnectInfo> RecentConnectionInfo { get; set; }
        List<string> VisualClientIPs { get; set; }
        string SimulatorIP { get; set; }
        string SimulatorPort { get; set; }
        bool FlashTaskbarPrivateMessage { get; set; }
        bool FlashTaskbarRadioMessage { get; set; }
        bool FlashTaskbarSelcal { get; set; }
        bool FlashTaskbarDisconnect { get; set; }
        bool KeepWindowVisible { get; set; }
        bool IsAudioConfigured { get; }
        WindowProperties ClientWindowProperties { get; set; }
        void LoadConfig(string path);
        void SaveConfig();
    }
}