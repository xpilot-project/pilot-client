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
using System.IO;
using System.Net;
using System.Reflection;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Events.Arguments;

namespace Vatsim.Xpilot.Core
{
    public enum VersionChannel
    {
        Stable,
        Beta,
        VelocityAlpha,
        VelocityBeta
    }

    public class VersionCheck : EventBus, IVersionCheck
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        private readonly IAppConfig mConfig;
        private const string ROOT_URL = "http://xpilot-project.org";
        private const string VERSION_CHECK_ENDPOINT = "api/v3/VersionCheck";
        private const string AIRCRAFT_DB_ENDPOINT = "api/v3/TypeCodes";

        public VersionCheck(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
        }


        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnInitiateVersionCheck(object sender, EventArgs e)
        {
            PerformVersionCheck();
            AircraftTypeCodeUpdate();
        }

        private void PerformVersionCheck()
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Performing version check..."));

            string userVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            try
            {
                var dto = new VersionCheckRequest
                {
                    Channel = VersionChannel.VelocityAlpha.ToString(),
                    UserVersion = userVersion,
                    VolumeId = SystemIdentifier.GetSystemDriveVolumeId(),
                    Cid = mConfig.VatsimId,
                    CreatedAt = DateTime.UtcNow
                };

                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
                string json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

                var client = new RestClient(ROOT_URL);
                var request = new RestRequest(VERSION_CHECK_ENDPOINT, DataFormat.Json)
                {
                    Method = Method.POST
                };
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                var response = client.Execute(request);

                var data = JsonConvert.DeserializeObject<VersionCheckResponse>(response.Content);

                if (response.StatusCode == HttpStatusCode.OK && data != null)
                {
                    Version version = new Version(data.Version);
                    if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "A new version is available for download."));

                        using (var dlg = new UpdateForm())
                        {
                            dlg.NewVersion = data.InformationalVersion;
                            dlg.DownloadUrl = data.DownloadUrl;
                            dlg.ReleaseNotes = data.Notes;
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
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error performing version check: {0}", ex.Message));
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
                        var hash = Path.Combine(mConfig.AppPath, "TypeCodes.json").CalculateHash();
                        if (hash != data.ChecksumHash)
                        {
                            using (WebClient wc = new WebClient())
                            {
                                var json = wc.DownloadString(data.TypeCodesUrl);
                                if (json != null)
                                {
                                    File.WriteAllText(Path.Combine(mConfig.AppPath, "TypeCodes.json"), json);
                                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Aircraft type designator database updated."));
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
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Aircraft type designator database updated."));
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error downloading aircraft type designator database: {ex.Message}"));
            }
        }
    }

    [Serializable]
    public class VersionCheckRequest
    {
        public string Channel { get; set; }
        public string UserVersion { get; set; }
        public string VolumeId { get; set; }
        public string Cid { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Serializable]
    public class VersionCheckResponse
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("informational_version")]
        public string InformationalVersion { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
        
        [JsonProperty("notes")]
        public string Notes { get; set; }
    }

    [Serializable]
    public class AircraftTypeCodes
    {
        [JsonProperty("TypeCodesUrl")]
        public string TypeCodesUrl { get; set; }

        [JsonProperty("Hash")]
        public string ChecksumHash { get; set; }
    }
}
