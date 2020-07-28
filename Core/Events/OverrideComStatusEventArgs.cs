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

namespace XPilot.PilotClient.Core.Events
{
    public class OverrideComStatusEventArgs : EventArgs
    {
        public bool Com1Rx { get; set; }
        public bool Com1Tx { get; set; }
        public bool Com2Rx { get; set; }
        public bool Com2Tx { get; set; }
        public OverrideComStatusEventArgs()
        {

        }
        public OverrideComStatusEventArgs(bool com1Rx, bool com1Tx, bool com2Rx, bool com2Tx) : this()
        {
            Com1Rx = com1Rx;
            Com1Tx = com1Tx;
            Com2Rx = com2Rx;
            Com2Tx = com2Tx;
        }
    }
}
