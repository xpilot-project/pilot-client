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
using Vatsim.Xpilot.Common;

namespace Vatsim.Xpilot.Aircrafts
{
    public class NetworkAircraftState
    {
        public long Timestamp { get; set; }
        public WorldPoint Location { get; set; }
        public double Altitude { get; set; }
        public double ReportedAltitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Heading { get; set; }
        public double GroundSpeed { get; set; }
    }
}
