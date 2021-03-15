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
using System.Globalization;

namespace Vatsim.Xpilot.Common
{
    public static class FrequencyUtils
    {
        public static string FormatFromNetwork(this uint frequency)
        {
            return ((double)frequency / 1000.0 + 100.0).ToString("0.000", CultureInfo.InvariantCulture);
        }

        public static bool IsValidFrequency(this uint frequency)
        {
            return frequency >= 18000 && frequency <= 36975;
        }

        public static int MatchNetworkFormat(this uint frequency)
        {
            int result;
            int.TryParse(frequency.ToString().Substring(1, frequency.ToString().Length - 1), out result);
            return result / 1000;
        }

        public static uint ToHertz(this uint freq)
        {
            return freq * 100000;
        }

        public static uint FsdFrequencyToHertz(this int freq)
        {
            return (uint)(freq + 100000) * 1000;
        }

        public static uint FsdFrequencyToHertz(this uint freq)
        {
            return (freq + 100000) * 1000;
        }

        public static uint Normalize25KhzFsdFrequency(this uint freq)
        {
            if (freq % 100 == 20 || freq % 100 == 70)
            {
                freq += 5;
            }
            return freq;
        }

        public static uint Normalize25KhzFrequency(this uint freq)
        {
            if (((freq % 1000) == 200) || ((freq % 1000) == 700))
            {
                freq += 50;
            }
            return freq;
        }

        public static uint UnNormalize25KhzFrequency(this uint freq)
        {
            uint freqint = freq / 1000;

            if (((freqint % 100) == 25) || ((freqint % 100) == 75))
            {
                freqint -= 5;
            }

            return freqint * 1000;
        }
    }
}
