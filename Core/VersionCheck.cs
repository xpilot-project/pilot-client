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
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;

namespace XPilot.PilotClient.Core
{
    public class VersionCheck : EventBus, IVersionCheck
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        private readonly IAppConfig mConfig;
        private const string ROOT_URL = "http://xpilot-project.org";
        private const string VERSION_CHECK_ENDPOINT = "api/v2/VersionCheck";
        private const string AIRCRAFT_DB_ENDPOINT = "api/v2/TypeCodes";

        public VersionCheck(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
        }

        private void XPilotVersionCheck()
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Performing version check..."));

            try
            {
                var dto = new VersionCheckDto
                {
                    UserVersion = Application.ProductVersion.ToString(),
                    Channel = mConfig.UpdateChannel.GetDescription()
                }.ToJSON();

                var client = new RestClient(ROOT_URL);
                var request = new RestRequest(VERSION_CHECK_ENDPOINT, DataFormat.Json);
                request.Method = Method.POST;
                request.AddParameter("application/json", dto, ParameterType.RequestBody);
                var response = client.Execute(request).Content;

                var data = JsonConvert.DeserializeObject<VersionCheckResponse>(response);

                if (data != null)
                {
                    Version version = new Version(data.Version);
                    if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "A new version is available for download."));

                        using (var dlg = new UpdateForm())
                        {
                            dlg.NewVersion = version.ToString();
                            dlg.DownloadUrl = data.VersionUrl;
                            dlg.ShowDialog();
                        }
                    }
                    else
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Version check complete. You are running the latest version."));
                    }
                }
                else
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Version check complete. You are running the latest version."));
                }
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error performing version check: {ex.Message}"));
            }
        }

        private void AircraftTypeCodeUpdate()
        {
            try
            {
                var client = new RestClient(ROOT_URL);
                var request = new RestRequest(AIRCRAFT_DB_ENDPOINT, DataFormat.Json);
                var response = client.Execute(request).Content;
                var data = JsonConvert.DeserializeObject<AircraftTypeCodes>(response);

                if (data != null)
                {
                    if (File.Exists(Path.Combine(mConfig.AppPath, "TypeCodes.json")))
                    {
                        var hash = FileChecksum.CalculateCheckSum(Path.Combine(mConfig.AppPath, "TypeCodes.json"));
                        if (hash != data.ChecksumHash)
                        {
                            using (WebClient wc = new WebClient())
                            {
                                var json = wc.DownloadString(data.TypeCodesUrl);
                                if (json != null)
                                {
                                    File.WriteAllText(Path.Combine(mConfig.AppPath, "TypeCodes.json"), json);
                                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Aircraft type code database updated."));
                                }
                            }
                        }
                    }
                    else
                    {
                        using (WebClient wc = new WebClient())
                        {
                            var json = wc.DownloadString(data.TypeCodesUrl);
                            if (json != null)
                            {
                                File.WriteAllText(Path.Combine(mConfig.AppPath, "TypeCodes.json"), json);
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Aircraft type code database updated."));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error downloading aircraft type code database: {ex.Message}"));
            }
        }


        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnInitiateVersionCheck(object sender, EventArgs e)
        {
            if (mConfig.CheckForUpdates && !Debugger.IsAttached)
                XPilotVersionCheck();

            AircraftTypeCodeUpdate();
        }
    }

    [Serializable]
    public class VersionCheckResponse
    {
        public string Version { get; set; }
        public string VersionUrl { get; set; }
        public long Timestamp { get; set; }
        public string Hash { get; set; }
        public string Channel { get; set; }
    }

    [Serializable]
    public class VersionCheckDto
    {
        public string Channel { get; set; }
        public string UserVersion { get; set; }
    }

    [Serializable]
    public class AircraftTypeCodes
    {
        [JsonProperty("TypeCodesUrl")]
        public string TypeCodesUrl { get; set; }

        [JsonProperty("Hash")]
        public string ChecksumHash { get; set; }
    }

    [Serializable]
    public class ClientFile
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("hash")]
        public string ChecksumHash { get; set; }

        [JsonProperty("channel")]
        public string UpdateChannel { get; set; }
    }

    [Serializable]
    public class AircraftFile
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("hash")]
        public string ChecksumHash { get; set; }
    }
}
