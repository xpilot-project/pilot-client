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
using System.Text;
using System.Threading.Tasks;
using Vatsim.Xpilot.Common;

namespace Vatsim.Xpilot.Aircrafts
{
    public enum AircraftStatus
    {
        New,
        Active,
        OutOfRange,
        Deleted
    }

    public class NetworkAircraft
    {
        public AircraftStatus Status { get; set; }
        public string Callsign { get; set; }
        public string Equipment { get; set; }
        public string Airline { get; set; }
        public List<NetworkAircraftPose> StateHistory { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastFlightPlanFetch { get; set; }
        public int UpdateCount { get; set; }
        public double GroundSpeed { get; set; }
        public int VerticalSpeed { get; set; }
        public LegacyClientConfigFlags LastAppliedConfigFlags { get; set; }
        public AircraftConfiguration Configuration { get; set; }
        public AircraftConfiguration LastAppliedConfiguration { get; set; }
        public bool SupportsConfigurationProtocol { get; set; }
        public bool InitialConfigurationSet { get; set; }
        public Stack<AircraftConfiguration> PendingAircraftConfiguration { get; set; }
        public NetworkAircraftTransponder Transponder { get; set; }
        public string OriginAirport { get; set; } = "";
        public string DestinationAirport { get; set; } = "";
        public NetworkAircraftPose CurrentPosition
        {
            get
            {
                return StateHistory.Last();
            }
        }
        public NetworkAircraftPose PreviousPosition
        {
            get
            {
                return StateHistory.First();
            }
        }
        public bool IsClimbing
        {
            get
            {
                return VerticalSpeed > 300;
            }
        }
        public bool IsDescending
        {
            get
            {
                return VerticalSpeed < -300;
            }
        }
        public NetworkAircraft()
        {
            StateHistory = new List<NetworkAircraftPose>();
            LastAppliedConfigFlags = new LegacyClientConfigFlags();
            PendingAircraftConfiguration = new Stack<AircraftConfiguration>();
            Transponder = new NetworkAircraftTransponder();
        }
        public override string ToString()
        {
            return Callsign;
        }
    }
}
