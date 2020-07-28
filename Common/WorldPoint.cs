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

namespace XPilot.PilotClient.Common
{
    public struct WorldPoint
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public WorldPoint(double lon, double lat)
        {
            this = default;
            Lat = lat;
            Lon = lon;
        }
        private static double RadiansToDegrees(double radians)
        {
            return radians / Math.PI * 180.0;
        }
        private static double DegreesToRadians(double degrees)
        {
            return degrees * 0.0174532925;
        }
        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Lat, this.Lon);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is WorldPoint other)) return false;
            return other.Lat == Lat && other.Lon == Lon;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
