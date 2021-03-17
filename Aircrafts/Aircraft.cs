﻿/*
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

namespace Vatsim.Xpilot.Aircrafts
{
    public enum AircraftStatus
    {
        New,
        Active,
        Ignored,
        Pending
    }

    public class Aircraft
    {
        public string Callsign { get; set; }
        public string Airline { get; set; }
        public string TypeCode { get; set; }
        public AircraftConfiguration Configuration { get; set; }
        public AircraftVisualState RemoteVisualState { get; set; }
        public DateTime LastSlowPositionUpdate { get; set; }
        public AircraftStatus Status { get; set; }
        public bool HaveVelocities { get; set; }
        public Aircraft(string callsign)
        {
            Callsign = callsign;
        }
    }
}