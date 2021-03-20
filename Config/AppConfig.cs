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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Networking;
using Vatsim.FsdClient;

namespace Vatsim.Xpilot.Config
{
    public class AppConfig : IAppConfig
    {
        [JsonIgnore]
        private string mPath = "";

        [JsonIgnore]
        public string AppPath { get; set; }

        [JsonIgnore]
        public string AfvResourcePath => Path.Combine(AppPath, "afv");

        [JsonIgnore]
        public Dictionary<int, string> AudioDrivers { get; set; }

        [JsonIgnore]
        public Dictionary<int, string> OutputDevices { get; set; }

        [JsonIgnore]
        public Dictionary<int, string> InputDevices { get; set; }

        [JsonIgnore]
        public bool SquawkingModeC { get; set; }

        [JsonIgnore]
        public bool ConfigurationRequired
        {
            get
            {
                return string.IsNullOrEmpty(VatsimId) || string.IsNullOrEmpty(VatsimPasswordDecrypted) || string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(Name);
            }
        }

        [JsonIgnore]
        public bool IsAudioConfigured
        {
            get
            {
                return !string.IsNullOrEmpty(InputDeviceName) && !string.IsNullOrEmpty(ListenDeviceName);
            }
        }

        public List<NetworkServerInfo> CachedServers { get; set; }
        public string VatsimId { get; set; }
        public string VatsimPassword { get; set; }
        [JsonIgnore]
        public string VatsimPasswordDecrypted
        {
            get
            {
                return Decrypt(VatsimPassword);
            }
            set
            {
                VatsimPassword = Encrypt(value);
            }
        }
        public string Name { get; set; }
        [JsonIgnore]
        public string NameWithAirport
        {
            get
            {
                if (!string.IsNullOrEmpty(HomeAirport))
                {
                    return $"{Name} ({HomeAirport})";
                }
                return Name;
            }
        }
        public string HomeAirport { get; set; }
        public string ServerName { get; set; }
        public bool AutoSquawkModeC { get; set; }
        public string AudioDriver { get; set; }
        public string InputDeviceName { get; set; }
        public string ListenDeviceName { get; set; }
        public bool DisableAudioEffects { get; set; }
        public bool EnableHfSquelch { get; set; }
        public bool EnableNotificationSounds { get; set; }
        public int Com1Volume { get; set; } = 100;
        public int Com2Volume { get; set; } = 100;
        public List<ConnectInfo> RecentConnectionInfo { get; set; }
        public string SimulatorIP { get; set; }
        public List<string> VisualClientIPs { get; set; }
        public string SimulatorPort { get; set; } = "52300";
        public bool FlashTaskbarPrivateMessage { get; set; }
        public bool FlashTaskbarRadioMessage { get; set; }
        public bool FlashTaskbarSelcal { get; set; }
        public bool FlashTaskbarDisconnect { get; set; }
        public bool KeepWindowVisible { get; set; }
        public WindowProperties ClientWindowProperties { get; set; }

        public AppConfig()
        {
            CachedServers = new List<NetworkServerInfo>();
            AudioDrivers = new Dictionary<int, string>();
            OutputDevices = new Dictionary<int, string>();
            InputDevices = new Dictionary<int, string>();
            RecentConnectionInfo = new List<ConnectInfo>();
            ClientWindowProperties = new WindowProperties();
            VisualClientIPs = new List<string>();

            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFilePath = Path.Combine(appPath, "AppConfig.json");

            try
            {
                LoadConfig(configFilePath);
            }
            catch (FileNotFoundException)
            {
                SaveConfig();
            }
            catch (Exception)
            {
                SaveConfig();
            }
            finally
            {
                AppPath = appPath;
            }
        }

        public void LoadConfig(string path)
        {
            try
            {
                var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fs))
                {
                    JsonConvert.PopulateObject(sr.ReadToEnd(), this);
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            finally
            {
                mPath = path;
            }
        }

        public void SaveConfig()
        {
            File.WriteAllText(mPath, JsonConvert.SerializeObject(this, Formatting.Indented));
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
                return new Guid(0xa1afdec5, 0x1f5c, 0x498b, 0xb4, 0xf1, 0x83, 0x49, 0x50, 0xfb, 0xea, 0xf0).ToString();
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
