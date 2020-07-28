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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XPilot.PilotClient.Config
{
    public enum PTTDeviceType
    {
        None,
        Keyboard,
        Joystick
    }

    public class PTTConfiguration : IEquatable<PTTConfiguration>
    {
        private string mJoystickGuidString;
        private Guid mGuid;

        public PTTDeviceType DeviceType { get; set; }

        public string Name { get; set; }

        public int ButtonOrKey { get; set; }

        [JsonIgnore]
        public bool JoystickAcquired { get; set; }

        public Guid JoystickGuid
        {
            get
            {
                return mGuid;
            }
        }

        public string JoystickGuidString
        {
            get
            {
                return mJoystickGuidString;
            }
            set
            {
                mJoystickGuidString = value;
                if (!string.IsNullOrEmpty(value))
                {
                    mGuid = new Guid(value);
                }
            }
        }

        public override string ToString()
        {
            switch (DeviceType)
            {
                case PTTDeviceType.Keyboard:
                    return $"Keyboard: { (Keys)ButtonOrKey }";
                case PTTDeviceType.Joystick:
                    return $"Joystick Button { ButtonOrKey + 1 } on { Name }";
                case PTTDeviceType.None:
                default:
                    return "None";
            }
        }

        public bool Equals(PTTConfiguration other)
        {
            return
                (other != null) &&
                (other.DeviceType == DeviceType) &&
                (other.Name == Name) &&
                (other.JoystickGuidString == JoystickGuidString) &&
                (other.ButtonOrKey == ButtonOrKey);
        }
    }
}
