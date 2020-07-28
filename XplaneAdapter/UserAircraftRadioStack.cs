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

namespace XPilot.PilotClient.XplaneAdapter
{
    public class UserAircraftRadioStack
    {
        public uint ComAudioSelection { get; set; }
        public bool AvionicsPower { get; set; }

        public uint Com1ActiveFreq { get; set; }
        public uint Com1StandbyFreq { get; set; }
        public bool Com1Power { get; set; }
        public float Com1Volume { get; set; }
        public bool Com1Listening { get; set; }

        public uint Com2ActiveFreq { get; set; }
        public uint Com2StandbyFreq { get; set; }
        public bool Com2Power { get; set; }
        public float Com2Volume { get; set; }
        public bool Com2Listening { get; set; }

        public bool Com1ForceRx { get; set; }
        public bool Com2ForceRx { get; set; }
        public bool Com1ForceTx { get; set; }
        public bool Com2ForceTx { get; set; }

        public bool HasPower
        {
            get
            {
                return AvionicsPower || Com1ForceRx || Com2ForceRx;
            }
        }

        public bool ReceivingOnBothFrequencies
        {
            get
            {
                return IsCom1Receiving && IsCom2Receiving;
            }
        }

        public bool IsCom1Selected
        {
            get
            {
                return ComAudioSelection == 6;
            }
        }

        public bool IsCom2Selected
        {
            get
            {
                return ComAudioSelection == 7;
            }
        }

        public bool IsCom1Receiving
        {
            get
            {
                return AvionicsPower && Com1Listening || Com1ForceRx;
            }
        }

        public bool IsCom2Receiving
        {
            get
            {
                return AvionicsPower && Com2Listening || Com2ForceRx;
            }
        }

        public bool IsCom1Transmitting
        {
            get
            {
                return AvionicsPower && IsCom1Selected || Com1ForceTx;
            }
        }

        public bool IsCom2Transmitting
        {
            get
            {
                return AvionicsPower && IsCom2Selected || Com2ForceTx;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UserAircraftRadioStack other)) return false;
            return ComAudioSelection == other.ComAudioSelection &&
               Com1ActiveFreq == other.Com1ActiveFreq &&
               Com1StandbyFreq == other.Com1StandbyFreq &&
               Com1Power == other.Com1Power &&
               Com1Volume == other.Com1Volume &&
               Com1Listening == other.Com1Listening &&
               Com2ActiveFreq == other.Com2ActiveFreq &&
               Com2StandbyFreq == other.Com2StandbyFreq &&
               Com2Power == other.Com2Power &&
               Com2Volume == other.Com2Volume &&
               Com2Listening == other.Com2Listening &&
               Com1ForceRx == other.Com1ForceRx &&
               Com1ForceTx == other.Com1ForceTx &&
               Com2ForceRx == other.Com2ForceRx &&
               Com2ForceTx == other.Com2ForceTx &&
               AvionicsPower == other.AvionicsPower;
        }
    }
}
