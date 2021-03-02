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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vatsim.Xpilot.Aircrafts
{
    [Serializable]
    public class NetworkAircraftConfig
    {
        [JsonProperty("Callsign")]
        public string Callsign { get; set; }

        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("Lights")]
        public RemoteAircraftLights Lights { get; set; }

        [JsonProperty("GearDown")]
        public bool? GearDown { get; set; }

        [JsonProperty("OnGround")]
        public bool? OnGround { get; set; }

        [JsonProperty("Flaps")]
        public float? Flaps { get; set; }

        [JsonProperty("EnginesOn")]
        public bool? EnginesOn { get; set; }

        [JsonProperty("SpoilersDeployed")]
        public bool? SpoilersDeployed { get; set; }

        internal NetworkAircraftConfig()
        {
            Lights = new RemoteAircraftLights();
        }

        internal bool HasConfig
        {
            get
            {
                return Lights != null && Lights.HasLightValues || GearDown.HasValue || OnGround.HasValue || Flaps.HasValue || EnginesOn.HasValue || SpoilersDeployed.HasValue;
            }
        }
    }

    [Serializable]
    public class RemoteAircraftLights
    {
        [JsonProperty("Strobes")]
        public bool? StrobesOn { get; set; }

        [JsonProperty("Landing")]
        public bool? LandingOn { get; set; }

        [JsonProperty("Taxi")]
        public bool? TaxiOn { get; set; }

        [JsonProperty("Beacon")]
        public bool? BeaconOn { get; set; }

        [JsonProperty("Nav")]
        public bool? NavOn { get; set; }

        internal bool HasLightValues
        {
            get
            {
                return StrobesOn.HasValue || LandingOn.HasValue || TaxiOn.HasValue || BeaconOn.HasValue || NavOn.HasValue;
            }
        }
    }
}
