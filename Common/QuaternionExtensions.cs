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
using Abacus.DoublePrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vatsim.Xpilot.Common
{
    static class QuaternionExtensions
    {
        internal static void ExtractEulerAngles(this Quaternion q, out double pitch, out double heading, out double bank)
        {
            double q0 = q.U, q1 = q.K, q2 = q.I, q3 = q.J,
                test = q0 * q2 - q3 * q1;
            bank = 0.0;
            if (test > .4999999999999999)
            {
                pitch = Math.PI / 2;
                heading = 2 * Math.Atan2(-q1, q0);
            }
            else if (test < -.4999999999999999)
            {
                pitch = -Math.PI / 2;
                heading = -2 * Math.Atan2(-q1, q0);
            }
            else
            {
                pitch = Math.Asin(2 * test);
                heading = Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (q2 * q2 + q3 * q3));
                bank = Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (q1 * q1 + q2 * q2));
            }
        }
    }
}
