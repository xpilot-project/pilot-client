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

namespace XPilot.PilotClient.Core.Events
{
    public enum MessageColor
    {
        White,
        Red,
        Orange,
        Yellow,
        Green,
        Gray
    }

    public class SimulatorMessageSent : EventArgs
    {
        public string Message { get; set; }
        public bool Direct { get; set; }
        public MessageColor Color { get; set; }
        public SimulatorMessageSent(string message) : this(message, false, MessageColor.White)
        {
            Message = message;
        }
        public SimulatorMessageSent(string message, bool direct) : this(message, direct, MessageColor.White)
        {
            Message = message;
            Direct = direct;
        }
        public SimulatorMessageSent(string message, MessageColor color) : this(message, false, color)
        {
            Message = message;
            Color = color;
        }
        public SimulatorMessageSent(string message, bool direct, MessageColor color)
        {
            Message = message;
            Direct = direct;
            Color = color;
        }
    }
}
