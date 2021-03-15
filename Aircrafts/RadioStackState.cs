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
    public struct RadioStackState
    {
        public bool AvionicsPowerOn;
        public uint Com1ActiveFrequency;
        public bool Com1TransmitEnabled;
        public bool Com1ReceiveEnabled;
        public uint Com2ActiveFrequency;
        public bool Com2TransmitEnabled;
        public bool Com2ReceiveEnabled;
        public bool SquawkingModeC;
        public ushort TransponderCode;
        public bool SquawkingIdent;
        public bool ReceivingOnBothComRadios => Com1ReceiveEnabled && Com2ReceiveEnabled;

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