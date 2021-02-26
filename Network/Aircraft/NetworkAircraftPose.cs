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
using XPilot.PilotClient.Common;

namespace XPilot.PilotClient.Aircraft
{
    public class NetworkAircraftPose
    {
        public WorldPoint Location { get; set; }
        public double Altitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Heading { get; set; }
        public double GroundSpeed { get; set; }
        public NetworkAircraftTransponder Transponder { get; set; }
        public double VerticalSpeed { get; set; }
        public NetworkAircraftPose()
        {
            Transponder = new NetworkAircraftTransponder();
        }
        public NetworkAircraftPose Clone()
        {
            return new NetworkAircraftPose
            {
                Location = Location,
                Altitude = Altitude,
                Pitch = Pitch,
                Bank = Bank,
                Heading = Heading,
                GroundSpeed = GroundSpeed,
                Transponder = Transponder
            };
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is NetworkAircraftPose other)) return false;
            return (other.Location.Equals(Location)
                && other.Altitude == Altitude
                && other.Pitch == Pitch
                && other.Bank == Bank
                && other.Heading == Heading
                && other.GroundSpeed == GroundSpeed
                && other.Transponder.Equals(Transponder));
        }
    }
}
