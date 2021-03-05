/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2021 Justin Shannon
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

namespace Vatsim.Xpilot.Aircrafts
{
    public class UserAircraftConfigData
    {
        public bool StrobesOn { get; set; }
        public bool LandingLightsOn { get; set; }
        public bool TaxiLightsOn { get; set; }
        public bool BeaconOn { get; set; }
        public bool NavLightOn { get; set; }
        public int EngineCount { get; set; }
        public bool Engine1Running { get; set; }
        public bool Engine2Running { get; set; }
        public bool Engine3Running { get; set; }
        public bool Engine4Running { get; set; }
        public bool GearDown { get; set; }
        public double FlapsRatio { get; set; }
        public double SpoilersRatio { get; set; }
        public bool OnGround { get; set; }
    }
}
