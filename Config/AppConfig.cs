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
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Network;
using GeoVR.Client;
using Newtonsoft.Json;
using Vatsim.Fsd.Connector;
using System.Reflection;

namespace XPilot.PilotClient.Config
{
    public enum UpdateChannel
    {
        [Description("Stable")]
        Stable,
        [Description("Beta")]
        Beta
    }

    public class AppConfig : IAppConfig
    {
        [JsonIgnore]
        private string mPath = "";

        [JsonIgnore]
        public string AppPath { get; set; }

        public List<NetworkServerInfo> CachedServers { get; set; }
        public string VatsimId { get; set; }
        public string VatsimPassword { get; set; }
        public string Name { get; set; }
        public string HomeAirport { get; set; }
        public string ServerName { get; set; }
        public List<string> VisualClientIPs { get; set; }
        public string SimClientIP { get; set; } = "";
        public List<ConnectInfo> RecentConnectionInfo { get; set; }
        public PTTConfiguration PTTConfiguration { get; set; }
        public ToggleDisplayConfiguration ToggleDisplayConfiguration { get; set; }
        public WindowProperties ClientWindowProperties { get; set; }
        public string InputDeviceName { get; set; }
        public string OutputDeviceName { get; set; }
        public bool DisableAudioEffects { get; set; }
        public bool HfSquelch { get; set; }
        public EqualizerPresets VhfEqualizer { get; set; } = EqualizerPresets.VHFEmulation2;
        public bool VolumeKnobsControlVolume { get; set; }
        public float Com1Volume { get; set; } = 1.0f;
        public float Com2Volume { get; set; } = 1.0f;
        public float InputVolumeDb { get; set; } = 0;
        public FlightPlan LastFlightPlan { get; set; }
        public bool FlashTaskbarPrivateMessage { get; set; } = true;
        public bool FlashTaskbarRadioMessage { get; set; }
        public bool FlashTaskbarSelCal { get; set; } = true;
        public bool FlashTaskbarDisconnect { get; set; } = true;
        public bool PlayGenericSelCalAlert { get; set; }
        public bool PlayRadioMessageAlert { get; set; }
        public bool AutoSquawkModeC { get; set; }
        public bool CheckForUpdates { get; set; } = true;
        public UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Stable;
        public bool KeepClientWindowVisible { get; set; }
        public int TcpPort { get; set; } = 45001;

        [JsonIgnore]
        public bool SquawkingModeC { get; set; }

        [JsonIgnore]
        public string NameWithAirport
        {
            get
            {
                if (!string.IsNullOrEmpty(HomeAirport))
                {
                    return $"{Name} {HomeAirport}";
                }
                return Name;
            }
        }

        [JsonIgnore]
        public bool ConfigurationRequired
        {
            get
            {
                return string.IsNullOrEmpty(VatsimId) || string.IsNullOrEmpty(VatsimPassword) || string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(Name);
            }
        }

        public AppConfig()
        {
            CachedServers = new List<NetworkServerInfo>();
            PTTConfiguration = new PTTConfiguration();
            ToggleDisplayConfiguration = new ToggleDisplayConfiguration();
            RecentConnectionInfo = new List<ConnectInfo>();
            VisualClientIPs = new List<string>();
            ClientWindowProperties = new WindowProperties();
        }

        public void LoadConfig(string path)
        {
            try
            {
                JsonConvert.PopulateObject(File.ReadAllText(path), this);
                DecryptConfig();
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            finally
            {
                mPath = path;
            }
        }

        public void SaveConfig()
        {
            EncryptConfig();
            File.WriteAllText(mPath, JsonConvert.SerializeObject(this, Formatting.Indented));
            DecryptConfig();
        }

        private void EncryptConfig()
        {
            try
            {
                VatsimPassword = Encrypt(VatsimPassword);
            }
            catch { }
        }

        private void DecryptConfig()
        {
            try
            {
                VatsimPassword = Decrypt(VatsimPassword);
            }
            catch { }
        }

        private string Encrypt(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();

                byte[] byteHash;
                byte[] byteBuff;

                byteHash = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                desCryptoProvider.Key = byteHash;
                desCryptoProvider.Mode = CipherMode.ECB;
                byteBuff = Encoding.UTF8.GetBytes(value);

                return Convert.ToBase64String(desCryptoProvider.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            return "";
        }

        private string Decrypt(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();

                byte[] byteHash;
                byte[] byteBuff;

                byteHash = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                desCryptoProvider.Key = byteHash;
                desCryptoProvider.Mode = CipherMode.ECB;
                byteBuff = Convert.FromBase64String(value);

                return Encoding.UTF8.GetString(desCryptoProvider.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            return "";
        }

        private static string EncryptionKey
        {
            get
            {
                return SystemIdentifier.GetSystemDriveVolumeId();
            }
        }

        [JsonIgnore]
        public object this[string propertyName]
        {
            get
            {
                Type mType = GetType();
                PropertyInfo mPropInfo = mType.GetProperty(propertyName);
                return mPropInfo.GetValue(this, null);
            }
            set
            {
                Type mType = GetType();
                PropertyInfo mPropInfo = mType.GetProperty(propertyName);
                mPropInfo.SetValue(this, value, null);
            }
        }
    }
}
