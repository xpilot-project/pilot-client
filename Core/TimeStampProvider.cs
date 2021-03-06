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
using System.Diagnostics;

namespace Vatsim.Xpilot.Core
{
    public class TimeStampProvider : ITimeStampProvider
    {
        private const string NORMAL_FORMAT = "HH:mm:ss";
        private const string PRECISE_FORMAT = "HH:mm:ss.fff";
        private const string FILENAME_FORMAT = "yyyyMMddHHmmss";

        private readonly long mTicksPerMs;

        public TimeStampProvider()
        {
            mTicksPerMs = Stopwatch.Frequency / 1000;
        }

        public static string Local => DateTime.Now.ToString(NORMAL_FORMAT);
        public static string LocalPrecise => DateTime.Now.ToString(PRECISE_FORMAT);
        public static string Zulu => DateTime.UtcNow.ToString(NORMAL_FORMAT);
        public static string ZuluPrecise => DateTime.UtcNow.ToString(PRECISE_FORMAT);
        public static string ForFileName => DateTime.Now.ToString(FILENAME_FORMAT);
        public long Precision => Stopwatch.GetTimestamp() / mTicksPerMs;
    }
}
