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
using Vatsim.Xpilot.Common;

namespace Vatsim.Xpilot.Controllers
{
    public class Controller
    {
        public string Callsign { get; set; }
        public uint Frequency { get; set; }
        public uint NormalizedFrequency { get; set; }
        public string RealName { get; set; }
        public bool IsValidATC { get; set; }
        public bool SupportsNewInfo { get; set; }
        public bool IsValid { get; set; }
        public WorldPoint Location { get; set; }
        public DateTime LastUpdate { get; set; }

        public override string ToString()
        {
            return Callsign;
        }
    }
}
