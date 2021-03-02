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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vatsim.Xpilot.Networking
{
    public class VatsimData
    {
        [JsonProperty("clients")]
        public List<FsdClient> Clients { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class FsdClient
    {
        public string Callsign { get; set; }
        public string Cid { get; set; }
        public string Realname { get; set; }
        public string Clienttype { get; set; }
        public string Frequency { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Altitude { get; set; }
        public int Groundspeed { get; set; }
        public string PlannedAircraft { get; set; }
        public string PlannedTascruise { get; set; }
        public string PlannedDepairport { get; set; }
        public string PlannedAltitude { get; set; }
        public string PlannedDestairport { get; set; }
        public string Server { get; set; }
        public int Protrevision { get; set; }
        public int Rating { get; set; }
        public int Transponder { get; set; }
        public int Facilitytype { get; set; }
        public int Visualrange { get; set; }
        public string PlannedRevision { get; set; }
        public string PlannedFlighttype { get; set; }
        public string PlannedDeptime { get; set; }
        public string PlannedActdeptime { get; set; }
        public string PlannedHrsenroute { get; set; }
        public string PlannedMinenroute { get; set; }
        public string PlannedHrsfuel { get; set; }
        public string PlannedMinfuel { get; set; }
        public string PlannedAltairport { get; set; }
        public string PlannedRemarks { get; set; }
        public string PlannedRoute { get; set; }
        public double PlannedDepairportLat { get; set; }
        public double PlannedDepairportLon { get; set; }
        public double PlannedDestairportLat { get; set; }
        public double PlannedDestairportLon { get; set; }
        public string AtisMessage { get; set; }
        public DateTime TimeLastAtisReceived { get; set; }
        public DateTime TimeLogon { get; set; }
        public int Heading { get; set; }
        public double QnhIHg { get; set; }
        public int QnhMb { get; set; }
    }
}
