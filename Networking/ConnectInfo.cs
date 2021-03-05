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
namespace Vatsim.Xpilot.Networking
{
    public class ConnectInfo
    {
        public string Callsign { get; set; }
        public string TypeCode { get; set; }
        public string SelCalCode { get; set; }
        public bool ObserverMode { get; set; }
        public bool TowerViewMode { get; set; }

        public ConnectInfo()
        {
            Callsign = "";
            TypeCode = "";
            SelCalCode = "";
        }

        public ConnectInfo(string callsign, string typeCode, string selCalCode, bool observerMode = false, bool towerMode = false) : this()
        {
            Callsign = callsign;
            TypeCode = typeCode;
            SelCalCode = selCalCode;
            ObserverMode = observerMode;
            TowerViewMode = towerMode;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}){2}", Callsign, TypeCode, ObserverMode ? " - Observer Mode" : "");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConnectInfo other)) return false;
            return other.Callsign == Callsign
                && other.TypeCode == TypeCode
                && other.SelCalCode == SelCalCode;
        }
    }
}
