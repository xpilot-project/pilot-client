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
namespace Vatsim.Xpilot.Aircrafts
{
    public class UserAircraftData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double AltitudeTrue { get; set; }
        public double AltitudePressure { get; set; }
        public double AltitudeAgl { get; set; }
        public double SpeedGround { get; set; }
        public double Pitch { get; set; }
        public double Heading { get; set; }
        public double Bank { get; set; }
        public double LatitudeVelocity { get; set; }
        public double AltitudeVelocity { get; set; }
        public double LongitudeVelocity { get; set; }
        public double PitchVelocity { get; set; }
        public double HeadingVelocity { get; set; }
        public double BankVelocity { get; set; }
    }
}
