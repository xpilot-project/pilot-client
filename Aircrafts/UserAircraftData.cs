﻿/*
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
    public struct UserAircraftData
    {
        public double Latitude;
        public double Longitude;
        public double AltitudeMslM;
        public double AltitudeAglM;
        public double SpeedGround;
        public double Pitch;
        public double Heading;
        public double Bank;
        public double LatitudeVelocity;
        public double AltitudeVelocity;
        public double LongitudeVelocity;
        public double PitchVelocity;
        public double HeadingVelocity;
        public double BankVelocity;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}