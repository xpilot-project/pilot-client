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
    public class SimulatorMessageEventArgs : EventArgs
    {
        public string Text { get; set; }
        public bool PriorityMessage { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
        public SimulatorMessageEventArgs()
        {

        }
        public SimulatorMessageEventArgs(string text, int red = 255, int green = 255, int blue = 255, bool priorityMessage = false) : this()
        {
            Text = text;
            Red = red;
            Green = green;
            Blue = blue;
            PriorityMessage = priorityMessage;
        }
    }
}
