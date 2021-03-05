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
namespace Vatsim.Xpilot.Aircrafts
{
    public class RadioStackState
    {
        public bool MasterSwitchOn { get; set; }
        public bool AvionicsPowerOn { get; set; }
        public uint Com1ActiveFrequency { get; set; }
        public bool Com1TransmitEnabled { get; set; }
        public bool Com1ReceiveEnabled { get; set; }
        public uint Com2ActiveFrequency { get; set; }
        public bool Com2TransmitEnabled { get; set; }
        public bool Com2ReceiveEnabled { get; set; }
        public bool SquawkingModeC { get; set; }
        public ushort TransponderCode { get; set; }
        public bool SquawkingIdent { get; set; }
        public bool ReceivingOnBothComRadios
        {
            get
            {
                if (Com1ReceiveEnabled)
                {
                    return Com2ReceiveEnabled;
                }
                return false;
            }
        }
    }
}