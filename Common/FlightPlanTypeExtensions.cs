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

namespace XPilot.PilotClient.Common
{
    public static class FlightPlanTypeExtensions
    {
        public static char ToChar(this FlightPlanType type)
        {
            char result;
            switch (type)
            {
                case FlightPlanType.IFR:
                    result = 'I';
                    break;
                case FlightPlanType.VFR:
                    result = 'V';
                    break;
                case FlightPlanType.DVFR:
                    result = 'D';
                    break;
                case FlightPlanType.SVFR:
                    result = 'S';
                    break;
                default:
                    result = ' ';
                    break;
            }
            return result;
        }

        public static FlightPlanType FromString(this FlightPlanType type, string typeString)
        {
            string text = typeString.ToUpper();
            FlightPlanType result;
            switch (text)
            {
                case "IFR":
                case "I":
                    result = FlightPlanType.IFR;
                    break;
                case "VFR":
                case "V":
                    result = FlightPlanType.VFR;
                    break;
                case "DVFR":
                case "D":
                    result = FlightPlanType.DVFR;
                    break;
                case "SVFR":
                case "S":
                    result = FlightPlanType.SVFR;
                    break;
                default:
                    result = FlightPlanType.IFR;
                    break;
            }
            return result;
        }
    }
}
