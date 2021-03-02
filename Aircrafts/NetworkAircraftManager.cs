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
using System.Windows.Forms;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using XPilot.PilotClient.Core.Events;
using Vatsim.Xpilot.Simulator;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;

namespace Vatsim.Xpilot.Networking.Aircraft
{
    public class NetworkAircraftManager : EventBus, INetworkAircraftManager
    {
        private readonly List<NetworkAircraft> mNetworkAircraft = new List<NetworkAircraft>();
        private readonly IAppConfig mConfig;
        private readonly IFsdManager mFsdManager;
        private readonly IXplaneConnectionManager mXplane;
        private readonly Timer mCheckIdleAircraftTimer;

        public NetworkAircraftManager(IEventBroker broker, IAppConfig config, IFsdManager fsdManager, IXplaneConnectionManager xplane) : base(broker)
        {
            mConfig = config;
            mFsdManager = fsdManager;
            mXplane = xplane;

            mCheckIdleAircraftTimer = new Timer
            {
                Interval = 1000
            };
            mCheckIdleAircraftTimer.Tick += CheckIdleAircraftTimer_Tick;
        }

        private void NetworkAircraftUpdateReceived(string from, NetworkAircraftPose state)
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

        private void UpdateExistingNetworkAircraft(NetworkAircraft aircraft, NetworkAircraftPose state)
        {
            if (aircraft != null && aircraft.UpdateCount >= 2 && string.IsNullOrEmpty(aircraft.Equipment))
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
                if (aircraft.SupportsConfigurationProtocol)
                {
                    while (aircraft.PendingAircraftConfiguration.Count > 0)
                    {
                        var cfg = aircraft.PendingAircraftConfiguration.Pop();
                        ProcessAircraftConfig(aircraft, cfg);
                    }
                }

                if ((DateTime.Now - aircraft.LastFlightPlanFetch).TotalMinutes >= 5)
                {
                    mFsdManager.RequestClientFlightPlan(aircraft.Callsign);
                    aircraft.LastFlightPlanFetch = DateTime.Now;
                }
            }
        }

        private void ProcessNewAircraft(string from, NetworkAircraftPose state)
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
            mXplane.AddPlane(aircraft);
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

        private void UpdateNetworkAircraftInSim(NetworkAircraft aircraft, NetworkAircraftPose pose)
        {
            mXplane.PlanePoseChanged(aircraft, pose);
        }

        private void DeleteNetworkAircraft(NetworkAircraft aircraft)
        {
            mXplane.RemovePlane(aircraft);
        }

        private void ProcessAircraftConfig(NetworkAircraft aircraft, AircraftConfiguration config)
        {
            Xpilot.AirplaneConfig cfg = new Xpilot.AirplaneConfig();
            cfg.Callsign = aircraft.Callsign;

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

            if (cfg.Lights == null)
            {
                cfg.Lights = new Xpilot.AirplaneConfig.Types.AirplaneConfigLights();
            }

            if (!aircraft.InitialConfigurationSet)
            {
                if (aircraft.Configuration.Lights.StrobesOn.HasValue)
                {
                    cfg.Lights.StrobeLightsOn = aircraft.Configuration.Lights.StrobesOn.Value;
                }
                if (aircraft.Configuration.Lights.LandingOn.HasValue)
                {
                    cfg.Lights.LandingLightsOn = aircraft.Configuration.Lights.LandingOn.Value;
                }
                if (aircraft.Configuration.Lights.TaxiOn.HasValue)
                {
                    cfg.Lights.TaxiLightsOn = aircraft.Configuration.Lights.TaxiOn.Value;
                }
                if (aircraft.Configuration.Lights.BeaconOn.HasValue)
                {
                    cfg.Lights.BeaconLightsOn = aircraft.Configuration.Lights.BeaconOn.Value;
                }
                if (aircraft.Configuration.Lights.NavOn.HasValue)
                {
                    cfg.Lights.NavLightsOn = aircraft.Configuration.Lights.NavOn.Value;
                }
                if (aircraft.Configuration.OnGround.HasValue)
                {
                    cfg.OnGround = aircraft.Configuration.OnGround.Value;
                }
                if (aircraft.Configuration.FlapsPercent.HasValue)
                {
                    cfg.Flaps = aircraft.Configuration.FlapsPercent.Value / 100.0f;
                }
                if (aircraft.Configuration.GearDown.HasValue)
                {
                    cfg.GearDown = aircraft.Configuration.GearDown.Value;
                }
                if (aircraft.Configuration.Engines.IsAnyEngineRunning)
                {
                    cfg.EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning;
                }
                if (aircraft.Configuration.SpoilersDeployed.HasValue)
                {
                    cfg.SpoilersDeployed = aircraft.Configuration.SpoilersDeployed.Value;
                }
                aircraft.InitialConfigurationSet = true;
            }
            else
            {
                if (aircraft.LastAppliedConfiguration == null)
                {
                    aircraft.InitialConfigurationSet = false;
                    mFsdManager.RequestFullAircraftConfiguration(aircraft.Callsign);
                    return;
                }

                if (aircraft.Configuration.Lights.StrobesOn.HasValue && aircraft.Configuration.Lights.StrobesOn.Value != aircraft.LastAppliedConfiguration.Lights.StrobesOn.Value)
                {
                    cfg.Lights.StrobeLightsOn = aircraft.Configuration.Lights.StrobesOn.Value;
                }
                if (aircraft.Configuration.Lights.NavOn.HasValue && aircraft.Configuration.Lights.NavOn.Value != aircraft.LastAppliedConfiguration.Lights.NavOn.Value)
                {
                    cfg.Lights.NavLightsOn = aircraft.Configuration.Lights.NavOn.Value;
                }
                if (aircraft.Configuration.Lights.BeaconOn.HasValue && aircraft.Configuration.Lights.BeaconOn.Value != aircraft.LastAppliedConfiguration.Lights.BeaconOn.Value)
                {
                    cfg.Lights.BeaconLightsOn = aircraft.Configuration.Lights.BeaconOn.Value;
                }
                if (aircraft.Configuration.Lights.LandingOn.HasValue && aircraft.Configuration.Lights.LandingOn.Value != aircraft.LastAppliedConfiguration.Lights.LandingOn.Value)
                {
                    cfg.Lights.LandingLightsOn = aircraft.Configuration.Lights.LandingOn.Value;
                }
                if (aircraft.Configuration.Lights.TaxiOn.HasValue && aircraft.Configuration.Lights.TaxiOn.Value != aircraft.LastAppliedConfiguration.Lights.TaxiOn.Value)
                {
                    cfg.Lights.TaxiLightsOn = aircraft.Configuration.Lights.TaxiOn.Value;
                }
                if (aircraft.Configuration.GearDown.HasValue && aircraft.Configuration.GearDown.Value != aircraft.LastAppliedConfiguration.GearDown.Value)
                {
                    cfg.GearDown = aircraft.Configuration.GearDown.Value;
                }
                if (aircraft.Configuration.OnGround.HasValue && aircraft.Configuration.OnGround.Value != aircraft.LastAppliedConfiguration.OnGround.Value)
                {
                    cfg.OnGround = aircraft.Configuration.OnGround.Value;
                }
                if (aircraft.Configuration.FlapsPercent.HasValue && aircraft.Configuration.FlapsPercent.Value != aircraft.LastAppliedConfiguration.FlapsPercent.Value)
                {
                    cfg.Flaps = aircraft.Configuration.FlapsPercent.Value / 100.0f;
                }
                if (aircraft.Configuration.SpoilersDeployed.HasValue && aircraft.Configuration.SpoilersDeployed.Value != aircraft.LastAppliedConfiguration.SpoilersDeployed.Value)
                {
                    cfg.SpoilersDeployed = aircraft.Configuration.SpoilersDeployed.Value;
                }
                if (aircraft.Configuration.Engines.IsAnyEngineRunning != aircraft.LastAppliedConfiguration.Engines.IsAnyEngineRunning)
                {
                    cfg.EnginesOn = aircraft.Configuration.Engines.IsAnyEngineRunning;
                }
            }
            aircraft.LastAppliedConfiguration = aircraft.Configuration.Clone();

            // override gear status if on ground... cuz some other pilot clients aren't sending correct data!
            if (!cfg.GearDown && cfg.OnGround)
            {
                cfg.GearDown = true;
            }

            mXplane.PlaneConfigChanged(cfg);
        }

        private void ProcessRemoteFlightPlan(FlightPlan fp)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == fp.Callsign);
            if (aircraft != null)
            {
                aircraft.OriginAirport = fp.DepartureAirport;
                aircraft.DestinationAirport = fp.DestinationAirport;
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnected e)
        {
            mCheckIdleAircraftTimer.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnected e)
        {
            mNetworkAircraft.Clear();
            mCheckIdleAircraftTimer.Stop();
        }

        [EventSubscription(EventTopics.CapabilitiesRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesRequestReceived(object sender, NetworkDataReceived e)
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
        public void OnCapabilitiesResponseReceived(object sender, NetworkDataReceived e)
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

        [EventSubscription(EventTopics.PlaneInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnNetworkAircraftInfoReceived(object sender, PlaneInfoReceived e)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.Callsign);
            if (aircraft == null)
            {
                Console.WriteLine($"Received aircraft info for an unknown aircraft: {e.Callsign}");
            }
            else if (!string.IsNullOrEmpty(aircraft.Equipment))
            {
                if (aircraft.Equipment != e.Equipment)
                {
                    // change model
                    aircraft.Equipment = e.Equipment;
                }
            }
            else
            {
                aircraft.Equipment = e.Equipment;
                aircraft.Airline = e.Airline;
                AddAircraftToSim(aircraft);
            }
        }

        [EventSubscription(EventTopics.AircraftConfigurationInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftConfigurationInfoReceived(object sender, NetworkDataReceived e)
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

        [EventSubscription(EventTopics.DeletePilotReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeletePilotReceived(object sender, NetworkDataReceived e)
        {
            NetworkAircraft aircraft = mNetworkAircraft.FirstOrDefault(o => o.Callsign == e.From);
            if (aircraft != null)
            {
                DeleteNetworkAircraft(aircraft);
            }
        }

        [EventSubscription(EventTopics.PilotPositionReceived, typeof(OnUserInterfaceAsync))]
        public void OnNetworkAircraftUpdateReceived(object sender, NetworkAircraftUpdateReceived e)
        {
            NetworkAircraftUpdateReceived(e.From, e.State);
        }

        [EventSubscription(EventTopics.RemoteFlightPlanReceived, typeof(OnUserInterfaceAsync))]
        public void OnRemoteFlightPlanReceived(object sender, FlightPlanReceived e)
        {
            ProcessRemoteFlightPlan(e.FlightPlan);
        }
    }
}