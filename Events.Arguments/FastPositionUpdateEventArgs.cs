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
using Abacus.DoublePrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vatsim.Xpilot.Aircrafts;

namespace Vatsim.Xpilot.Events.Arguments
{
    public class FastPositionUpdateEventArgs : NetworkDataReceivedEventArgs
    {
        public AircraftVisualState VisualState { get; set; }
        public Vector3 PositionalVelocityVector { get; set; }
        public Vector3 RotationalVelocityVector { get; set; }
        public FastPositionUpdateEventArgs(string from, AircraftVisualState visualState, Vector3 positionalVelocityVector, Vector3 rotationalVelocityVecotr) : base(from, null)
        {
            VisualState = visualState;
            PositionalVelocityVector = positionalVelocityVector;
            RotationalVelocityVector = rotationalVelocityVecotr;
        }
    }
}
