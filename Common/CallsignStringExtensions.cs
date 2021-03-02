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
    public static class CallsignStringExtensions
    {
        internal static bool FuzzyMatches(this string callsign, string compareTo)
        {
            GetPrefixSuffix(callsign, out string prefixA, out string suffixA);
            GetPrefixSuffix(compareTo, out string prefixB, out string suffixB);
            return (prefixA == prefixB) && (suffixA == suffixB);
        }

        private static void GetPrefixSuffix(string callsign, out string prefix, out string suffix)
        {
            string[] parts = callsign.Split(new char[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            prefix = parts.First();
            suffix = parts.Last();
        }
    }
}
