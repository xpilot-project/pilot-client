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
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;
using XPilot.PilotClient.Aircraft;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.XplaneAdapter;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Newtonsoft.Json;
using System.Net;

namespace XPilot.PilotClient.Network.Aircraft
{
    public class NetworkAircraftManager : EventBus, INetworkAircraftManager
    {
        [EventPublication(EventTopics.XPlaneEventPosted)]
        public event EventHandler<ClientEventArgs<string>> XPlaneEventPosted;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        private const string VATSIM_DATA_URL = "http://cluster.data.vatsim.net/vatsim-data.json";

        private readonly List<NetworkAircraft> mNetworkAircraft = new List<NetworkAircraft>();
        private readonly IAppConfig mConfig;
        private readonly IFsdManger mFsdManager;
        private readonly Timer mCheckIdleAircraftTimer;
        private readonly Timer mDownloadDataFeed;
        private VatsimData VatsimDatafeed;

        public NetworkAircraftManager(IEventBroker broker, IAppConfig config, IFsdManger fsdManager) : base(broker)
        {
            mConfig = config;
            mFsdManager = fsdManager;

            mCheckIdleAircraftTimer = new Timer
            {
                Interval = 500
            };
            mCheckIdleAircraftTimer.Tick += CheckIdleAircraftTimer_Tick;

            mDownloadDataFeed = new Timer
            {
                Interval = 60000
            };
            mDownloadDataFeed.Tick += RefreshVatsimData_Tick;
            LoadVatsimData();
        }

        private async void LoadVatsimData()
        {
            try
            {
                string json = await new WebClient() { Encoding = System.Text.Encoding.UTF8 }.DownloadStringTaskAsync(new Uri(VATSIM_DATA_URL));
                VatsimDatafeed = JsonConvert.DeserializeObject<VatsimData>(json);
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Error downloading VATSIM data:" + ex.Message));
            }
        }

        private void RefreshVatsimData_Tick(object sender, EventArgs e)
        {
            LoadVatsimData();
        }

        private void NetworkAircraftUpdateReceived(string from, NetworkAircraftState state)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == from);
            if (aircraft == null)
            {
                ProcessNewAircraft(from, state);
            }
            else
            {
                UpdateExistingNetworkAircraft(aircraft, state);
            }
        }

        private void UpdateExistingNetworkAircraft(NetworkAircraft aircraft, NetworkAircraftState state)
        {
            if (aircraft != null && aircraft.UpdateCount >= 2 && string.IsNullOrEmpty(aircraft.TypeCode))
            {
                Console.WriteLine($"Still no type code received for {aircraft.Callsign} after position update #{aircraft.UpdateCount} - requesting aircraft info");
                mFsdManager.RequestPlaneInformation(aircraft.Callsign);
            }

            double diff = (DateTime.Now - aircraft.LastUpdated).TotalSeconds;
            aircraft.VerticalSpeed = (int)((aircraft.CurrentPosition.Altitude - aircraft.PreviousPosition.Altitude) / (diff / 60.0));
            aircraft.LastUpdated = DateTime.Now;
            aircraft.StateHistory.Add(state);
            aircraft.Transponder = state.Transponder;
            aircraft.UpdateCount++;

            while (aircraft.StateHistory.Count > 2)
            {
                aircraft.StateHistory.RemoveAt(0);
            }

            UpdateNetworkAircraftInSim(aircraft, state);

            if (aircraft.Status == AircraftStatus.Active)
            {
                if (!aircraft.SupportsConfigurationProtocol)
                {
                    UpdateLegacyAircraftConfig(aircraft);
                }
                else
                {
                    while (aircraft.PendingAircraftConfiguration.Count > 0)
                    {
                        var cfg = aircraft.PendingAircraftConfiguration.Pop();
                        ProcessAircraftConfig(aircraft, cfg);
                    }
                }
            }
        }

        private void ProcessNewAircraft(string from, NetworkAircraftState state)
        {
            NetworkAircraft aircraft = new NetworkAircraft
            {
                Callsign = from,
                LastUpdated = DateTime.Now,
                UpdateCount = 1,
                Status = AircraftStatus.New
            };
            aircraft.StateHistory.Add(state);
            mNetworkAircraft.Add(aircraft);

            Console.WriteLine($"Aircraft discovered: {aircraft.Callsign} - requesting CAPS, sending CAPS, requesting aircraft info");
            mFsdManager.RequestClientCapabilities(aircraft.Callsign);
            mFsdManager.SendClientCaps(aircraft.Callsign);
            mFsdManager.RequestPlaneInformation(aircraft.Callsign);
        }

        private void AddAircraftToSim(NetworkAircraft aircraft)
        {
            aircraft.Status = AircraftStatus.Active;

            dynamic data = new ExpandoObject();
            data.Callsign = aircraft.Callsign;
            data.TypeCode = aircraft.TypeCode.Substring(0, Math.Min(4, aircraft.TypeCode.Length));
            if (aircraft.Callsign.Length >= 3)
            {
                data.Airline = aircraft.Callsign.Substring(0, 3);
            }

            XplaneConnect xp = new XplaneConnect
            {
                Type = XplaneConnect.MessageType.AddPlane,
                Timestamp = DateTime.Now,
                Data = data
            };
            XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(xp.ToJSON()));
        }

        private void CheckIdleAircraftTimer_Tick(object sender, EventArgs e)
        {
            foreach (var aircraft in mNetworkAircraft.ToList())
            {
                if (aircraft.Status == AircraftStatus.Active)
                {
                    if ((DateTime.Now - aircraft.LastUpdated).TotalSeconds >= 15)
                    {
                        DeleteNetworkAircraft(aircraft);
                    }
                }
            }
        }

        private void UpdateLegacyAircraftConfig(NetworkAircraft aircraft)
        {
            LegacyClientConfigFlags flags = new LegacyClientConfigFlags
            {
                EnginesRunning = true,
                BeaconLightsOn = true,
                NavLightsOn = true
            };

            if (aircraft.GroundSpeed >= 50.0)
            {
                flags.StrobeLightsOn = true;
                flags.TaxiLightsOn = false;
                flags.LandingLightsOn = aircraft.CurrentPosition.Altitude <= 10000 ? true : false;
            }
            else if (aircraft.GroundSpeed > 0.0)
            {
                flags.TaxiLightsOn = true;
                flags.StrobeLightsOn = false;
                flags.LandingLightsOn = false;
            }
            else
            {
                flags.StrobeLightsOn = false;
                flags.LandingLightsOn = false;
                flags.TaxiLightsOn = false;
            }
            flags.OnGround = aircraft.CurrentPosition.Altitude == aircraft.PreviousPosition.Altitude;
            flags.GearDown = DeriveGearStatus(aircraft);

            UpdateLegacyAircraftConfig(aircraft, flags);
        }

        private bool DeriveGearStatus(NetworkAircraft aircraft)
        {
            if (aircraft.IsClimbing && aircraft.CurrentPosition.Altitude > 500.0)
            {
                return false;
            }
            if (aircraft.IsDescending && aircraft.CurrentPosition.Altitude <= 3000.0)
            {
                return true;
            }
            if (aircraft.LastAppliedConfigFlags.OnGround)
            {
                return true;
            }
            if (aircraft.CurrentPosition.Altitude < 300.0)
            {
                return true;
            }
            return false;
        }

        private void UpdateNetworkAircraftInSim(NetworkAircraft aircraft, NetworkAircraftState State)
        {
            dynamic data = new ExpandoObject();
            data.Callsign = aircraft.Callsign;
            data.Latitude = State.Location.Lat;
            data.Longitude = State.Location.Lon;
            data.Altitude = State.Altitude;
            data.Bank = State.Bank;
            data.Pitch = State.Pitch;
            data.Heading = State.Heading;
            data.GroundSpeed = State.GroundSpeed;
            data.TransponderCode = State.Transponder.TransponderCode;
            data.TransponderModeC = State.Transponder.TransponderModeC;
            data.TransponderIdent = State.Transponder.TransponderIdent;
            data.Origin = "";
            data.Destination = "";

            if (VatsimDatafeed.Clients != null)
            {
                var client = VatsimDatafeed.Clients.FirstOrDefault(a => a.Callsign == aircraft.Callsign);
                if (client != null)
                {
                    data.Origin = client.PlannedDepairport ?? "";
                    data.Destination = client.PlannedDestairport ?? "";
                }
            }

            XplaneConnect xp = new XplaneConnect
            {
                Type = XplaneConnect.MessageType.PositionUpdate,
                Timestamp = DateTime.Now,
                Data = data
            };
            XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(xp.ToJSON()));
        }

        private void UpdateLegacyAircraftConfig(NetworkAircraft aircraft, LegacyClientConfigFlags flags)
        {
            NetworkAircraftConfig temp = new NetworkAircraftConfig
            {
                Callsign = aircraft.Callsign,
                Timestamp = DateTime.Now,
            };

            if (!aircraft.InitialConfigurationSet)
            {
                temp.GearDown = flags.GearDown;
                temp.OnGround = flags.OnGround;
                temp.Lights.TaxiOn = flags.TaxiLightsOn;
                temp.Lights.LandingOn = flags.LandingLightsOn;
                temp.Lights.BeaconOn = flags.BeaconLightsOn;
                temp.Lights.NavOn = flags.NavLightsOn;
                temp.Lights.StrobesOn = flags.StrobeLightsOn;
                temp.EnginesOn = flags.EnginesRunning;
                aircraft.InitialConfigurationSet = true;
            }
            else
            {
                if (flags.GearDown != aircraft.LastAppliedConfigFlags.GearDown)
                {
                    temp.GearDown = flags.GearDown;
                }
                if (flags.OnGround != aircraft.LastAppliedConfigFlags.OnGround)
                {
                    temp.OnGround = flags.OnGround;
                }
                if (flags.TaxiLightsOn != aircraft.LastAppliedConfigFlags.TaxiLightsOn)
                {
                    temp.Lights.TaxiOn = flags.TaxiLightsOn;
                }
                if (flags.LandingLightsOn != aircraft.LastAppliedConfigFlags.LandingLightsOn)
                {
                    temp.Lights.LandingOn = flags.LandingLightsOn;
                }
                if (flags.BeaconLightsOn != aircraft.LastAppliedConfigFlags.BeaconLightsOn)
                {
                    temp.Lights.BeaconOn = flags.BeaconLightsOn;
                }
                if (flags.NavLightsOn != aircraft.LastAppliedConfigFlags.NavLightsOn)
                {
                    temp.Lights.NavOn = flags.NavLightsOn;
                }
                if (flags.EnginesRunning != aircraft.LastAppliedConfigFlags.EnginesRunning)
                {
                    temp.EnginesOn = flags.EnginesRunning;
                }
            }
            aircraft.LastAppliedConfigFlags = flags;

            if (temp.HasConfig)
            {
                XplaneConnect msg = new XplaneConnect
                {
                    Type = XplaneConnect.MessageType.SurfaceUpdate,
                    Timestamp = DateTime.Now,
                    Data = temp
                };
                XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(msg.ToJSON()));
                Console.WriteLine($"Sending Legacy Config to the sim for: {aircraft.Callsign} - {msg.ToJSON()}");
            }
        }

        private void DeleteNetworkAircraft(NetworkAircraft aircraft)
        {
            dynamic data = new ExpandoObject();
            data.Callsign = aircraft.Callsign;

            XplaneConnect xp = new XplaneConnect
            {
                Type = XplaneConnect.MessageType.RemovePlane,
                Timestamp = DateTime.Now,
                Data = data
            };
            XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(xp.ToJSON()));
            mNetworkAircraft.Remove(aircraft);
        }

        private void ProcessAircraftConfig(NetworkAircraft aircraft, AircraftConfiguration config)
        {
            NetworkAircraftConfig cfgInterop = new NetworkAircraftConfig
            {
                Callsign = aircraft.Callsign,
                Timestamp = DateTime.Now
            };

            if (config.IsFullData.HasValue && config.IsFullData.Value)
            {
                config.EnsurePopulated();
                aircraft.SupportsConfigurationProtocol = true;
                aircraft.Configuration = config;
            }
            else
            {
                if (!aircraft.SupportsConfigurationProtocol || aircraft.Configuration == null)
                {
                    return;
                }
                aircraft.Configuration.ApplyIncremental(config);
            }

            if (!aircraft.InitialConfigurationSet)
            {
                if (aircraft.Configuration.Lights.StrobesOn.HasValue)
                {
                    cfgInterop.Lights.StrobesOn = aircraft.Configuration.Lights.StrobesOn.Value;
                }
                if (aircraft.Configuration.Lights.LandingOn.HasValue)
                {
                    cfgInterop.Lights.LandingOn = aircraft.Configuration.Lights.LandingOn.Value;
                }
                if (aircraft.Configuration.Lights.TaxiOn.HasValue)
                {
                    cfgInterop.Lights.TaxiOn = aircraft.Configuration.Lights.TaxiOn.Value;
                }
                if (aircraft.Configuration.Lights.BeaconOn.HasValue)
                {
                    cfgInterop.Lights.BeaconOn = aircraft.Configuration.Lights.BeaconOn.Value;
                }
                if (aircraft.Configuration.Lights.NavOn.HasValue)
                {
                    cfgInterop.Lights.NavOn = aircraft.Configuration.Lights.NavOn.Value;
                }
                if (aircraft.Configuration.OnGround.HasValue)
                {
                    cfgInterop.OnGround = aircraft.Configuration.OnGround.Value;
                }
                if (aircraft.Configuration.FlapsPercent.HasValue)
                {
                    cfgInterop.Flaps = aircraft.Configuration.FlapsPercent.Value / 100.0f;
                }
                if (aircraft.Configuration.GearDown.HasValue)
                {
                    cfgInterop.GearDown = aircraft.Configuration.GearDown.Value;
                }
                if (aircraft.Configuration.Engines.IsAnyEngineRunning)
                {
                    cfgInterop.EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning;
                }
                if (aircraft.Configuration.SpoilersDeployed.HasValue)
                {
                    cfgInterop.SpoilersDeployed = aircraft.Configuration.SpoilersDeployed.Value;
                }
                aircraft.InitialConfigurationSet = true;
            }
            else
            {
                if (aircraft.LastAppliedConfiguration == null)
                {
                    aircraft.InitialConfigurationSet = false;
                    Console.WriteLine($"LastAppliedConfiguration null; Requesting full ACCONFIG for: {aircraft.Callsign}");
                    mFsdManager.RequestFullAircraftConfiguration(aircraft.Callsign);
                    return;
                }

                if (aircraft.Configuration.Lights.StrobesOn.HasValue && aircraft.Configuration.Lights.StrobesOn.Value != aircraft.LastAppliedConfiguration.Lights.StrobesOn.Value)
                {
                    cfgInterop.Lights.StrobesOn = aircraft.Configuration.Lights.StrobesOn.Value;
                }
                if (aircraft.Configuration.Lights.NavOn.HasValue && aircraft.Configuration.Lights.NavOn.Value != aircraft.LastAppliedConfiguration.Lights.NavOn.Value)
                {
                    cfgInterop.Lights.NavOn = aircraft.Configuration.Lights.NavOn.Value;
                }
                if (aircraft.Configuration.Lights.BeaconOn.HasValue && aircraft.Configuration.Lights.BeaconOn.Value != aircraft.LastAppliedConfiguration.Lights.BeaconOn.Value)
                {
                    cfgInterop.Lights.BeaconOn = aircraft.Configuration.Lights.BeaconOn.Value;
                }
                if (aircraft.Configuration.Lights.LandingOn.HasValue && aircraft.Configuration.Lights.LandingOn.Value != aircraft.LastAppliedConfiguration.Lights.LandingOn.Value)
                {
                    cfgInterop.Lights.LandingOn = aircraft.Configuration.Lights.LandingOn.Value;
                }
                if (aircraft.Configuration.Lights.TaxiOn.HasValue && aircraft.Configuration.Lights.TaxiOn.Value != aircraft.LastAppliedConfiguration.Lights.TaxiOn.Value)
                {
                    cfgInterop.Lights.TaxiOn = aircraft.Configuration.Lights.TaxiOn.Value;
                }
                if (aircraft.Configuration.GearDown.HasValue && aircraft.Configuration.GearDown.Value != aircraft.LastAppliedConfiguration.GearDown.Value)
                {
                    cfgInterop.GearDown = aircraft.Configuration.GearDown.Value;
                }
                if (aircraft.Configuration.OnGround.HasValue && aircraft.Configuration.OnGround.Value != aircraft.LastAppliedConfiguration.OnGround.Value)
                {
                    cfgInterop.OnGround = aircraft.Configuration.OnGround.Value;
                }
                if (aircraft.Configuration.FlapsPercent.HasValue && aircraft.Configuration.FlapsPercent.Value != aircraft.LastAppliedConfiguration.FlapsPercent.Value)
                {
                    cfgInterop.Flaps = aircraft.Configuration.FlapsPercent.Value / 100.0f;
                }
                if (aircraft.Configuration.SpoilersDeployed.HasValue && aircraft.Configuration.SpoilersDeployed.Value != aircraft.LastAppliedConfiguration.SpoilersDeployed.Value)
                {
                    cfgInterop.SpoilersDeployed = aircraft.Configuration.SpoilersDeployed.Value;
                }
                if (aircraft.Configuration.Engines.IsAnyEngineRunning != aircraft.LastAppliedConfiguration.Engines.IsAnyEngineRunning)
                {
                    cfgInterop.EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning;
                }
            }
            aircraft.LastAppliedConfiguration = aircraft.Configuration.Clone();

            if (cfgInterop.HasConfig)
            {
                XplaneConnect xp = new XplaneConnect
                {
                    Type = XplaneConnect.MessageType.SurfaceUpdate,
                    Timestamp = DateTime.Now,
                    Data = cfgInterop
                };
                XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(xp.ToJSON()));
                Console.WriteLine($"Sending ACCONFIG to the sim for: {aircraft.Callsign} - {xp.ToJSON()}");
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mCheckIdleAircraftTimer.Start();
            mDownloadDataFeed.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            mNetworkAircraft.Clear();
            mCheckIdleAircraftTimer.Stop();
            mDownloadDataFeed.Stop();
        }

        [EventSubscription(EventTopics.CapabilitiesRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesRequestReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mNetworkAircraft.Any(a => a.Callsign == e.From))
            {
                Console.WriteLine($"Received capabilities request from {e.From} - requesting aircraft info");
                mFsdManager.RequestPlaneInformation(e.From);
            }
            else
            {
                Console.WriteLine($"Received capabilities request from unknown callsign: {e.From}");
            }
        }

        [EventSubscription(EventTopics.CapabilitiesResponseReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesResponseReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            Console.WriteLine($"Received capabilities list from: {e.From} - {e.Data}");
            if (e.Data.Contains("ACCONFIG=1"))
            {
                NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.From);
                if (aircraft != null)
                {
                    Console.WriteLine($"Remote aircraft supports ACCONFIG: {e.From}");
                    aircraft.SupportsConfigurationProtocol = true;

                    Console.WriteLine($"Requesting full ACCONFIG packet from {e.From}");
                    mFsdManager.RequestFullAircraftConfiguration(e.From);
                }
            }
        }

        [EventSubscription(EventTopics.NetworkAircraftInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnNetworkAircraftInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.From);
            if (aircraft == null)
            {
                Console.WriteLine($"Received aircraft info for an unknown aircraft: {e.From}");
            }
            else
            {
                if (!string.IsNullOrEmpty(aircraft.TypeCode))
                {
                    if (aircraft.TypeCode != e.Data)
                    {
                        Console.WriteLine($"Received new aircraft info for {e.From} - changing model");

                        aircraft.TypeCode = e.Data.IndexOf("-") > -1 ? e.Data.Substring(0, e.Data.IndexOf("-")) : e.Data;

                        dynamic data = new ExpandoObject();
                        data.Callsign = aircraft.Callsign;
                        data.TypeCode = aircraft.TypeCode.Substring(0, Math.Min(4, aircraft.TypeCode.Length));
                        data.Airline = aircraft.AirlineIcao;

                        XplaneConnect xp = new XplaneConnect
                        {
                            Type = XplaneConnect.MessageType.ChangeModel,
                            Timestamp = DateTime.Now,
                            Data = data
                        };
                        XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(xp.ToJSON()));
                    }
                    else
                    {
                        Console.WriteLine($"Received unchanged aircraft info for {e.From} - already have a model selected");
                    }
                }
                else
                {
                    aircraft.TypeCode = e.Data.IndexOf("-") > -1 ? e.Data.Substring(0, e.Data.IndexOf("-")) : e.Data;
                    AddAircraftToSim(aircraft);
                }
            }
        }

        [EventSubscription(EventTopics.AircraftConfigurationInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftConfigurationInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AircraftConfigurationInfo aircraftConfigurationInfo;
            try
            {
                aircraftConfigurationInfo = AircraftConfigurationInfo.FromJson(e.Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to deserialize aircraft configuration JSON from {e.From}: {e.Data} - Error: {ex.Message}");
                return;
            }
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.From);
            if (aircraftConfigurationInfo.HasConfig && aircraft != null)
            {
                if (!aircraft.SupportsConfigurationProtocol)
                {
                    aircraft.SupportsConfigurationProtocol = true;
                    mFsdManager.RequestFullAircraftConfiguration(aircraft.Callsign);
                    Console.WriteLine($"Received aircraft configuration from {e.From}. Requesting full ACCONFIG again.");
                }

                if (aircraft.Status == AircraftStatus.Active)
                {
                    Console.WriteLine($"Received aircraft configuration info from: {e.From}: {e.Data}");
                    ProcessAircraftConfig(aircraft, aircraftConfigurationInfo.Config);
                }
                else
                {
                    Console.WriteLine($"Queing ACCONFIG for {e.From} because the aircraft isn't active yet.");
                    aircraft.PendingAircraftConfiguration.Push(aircraftConfigurationInfo.Config);
                }
            }
        }

        [EventSubscription(EventTopics.LegacyPlaneInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnLegacyPlaneInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == e.From);
            if (aircraft != null)
            {
                if (aircraft.UpdateCount >= 3)
                {
                    if (string.IsNullOrEmpty(aircraft.TypeCode))
                    {
                        Console.WriteLine($"Received legacy aircraft info for {e.From} after {aircraft.UpdateCount} updates with no modern plane info packet received, selecting model based on legacy info.");
                        aircraft.TypeCode = e.Data.IndexOf("-") > -1 ? e.Data.Substring(0, e.Data.IndexOf("-")) : e.Data;
                        AddAircraftToSim(aircraft);
                    }
                    else
                    {
                        Console.WriteLine($"Received legacy aircraft info for {e.From} after having already selected a model, ignorning");
                    }
                }
                else
                {
                    Console.WriteLine($"Received legacy aircraft info for {e.From} after fewer than 3 updates, ignoring.");
                }
            }
            else
            {
                Console.WriteLine($"Received legacy aircraft info for unknown callsign {e.From}, ignoring.");
            }
        }

        [EventSubscription(EventTopics.DeletePilotReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeletePilotReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.From);
            if (aircraft != null)
            {
                DeleteNetworkAircraft(aircraft);
            }
        }

        [EventSubscription(EventTopics.NetworkAircraftUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnNetworkAircraftUpdateReceived(object sender, AircraftUpdateReceivedEventArgs e)
        {
            NetworkAircraftUpdateReceived(e.From, e.State);
        }
    }
}
