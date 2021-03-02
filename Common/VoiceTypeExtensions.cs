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

namespace Vatsim.Xpilot.Common
{
    public static class VoiceTypeExtensions
    {
        public static VoiceType FromString(this VoiceType vt, string typeID)
        {
            string a = typeID.ToLower();
            VoiceType result;
            switch (a)
            {
                case "t":
                    result = VoiceType.TextOnly;
                    break;
                case "r":
                    result = VoiceType.ReceiveOnly;
                    break;
                case "v":
                    result = VoiceType.Full;
                    break;
                default:
                    result = VoiceType.Unknown;
                    break;
            }
            return result;
        }


        public static string GetVoiceTypeID(this VoiceType vt)
        {
            string result;
            switch (vt)
            {
                case VoiceType.Full:
                    result = "v";
                    break;
                case VoiceType.ReceiveOnly:
                    result = "r";
                    break;
                case VoiceType.TextOnly:
                    result = "t";
                    break;
                default:
                    result = "?";
                    break;
            }
            return result;
        }

        public static VoiceType FromFlightPlanRemarks(this VoiceType vt, string remarks)
        {
            bool flag = remarks.ToLower().Contains("/v/");
            VoiceType result;
            if (flag)
            {
                result = VoiceType.Full;
            }
            else
            {
                bool flag2 = remarks.ToLower().Contains("/r/");
                if (flag2)
                {
                    result = VoiceType.ReceiveOnly;
                }
                else
                {
                    bool flag3 = remarks.ToLower().Contains("/t/");
                    if (flag3)
                    {
                        result = VoiceType.TextOnly;
                    }
                    else
                    {
                        result = VoiceType.Unknown;
                    }
                }
            }
            return result;
        }

        public static string GetCallsignTag(this VoiceType vt)
        {
            string result;
            switch (vt)
            {
                case VoiceType.Full:
                    result = "";
                    break;
                case VoiceType.ReceiveOnly:
                    result = "/R";
                    break;
                case VoiceType.TextOnly:
                    result = "/T";
                    break;
                default:
                    result = "/?";
                    break;
            }
            return result;
        }
    }
}
