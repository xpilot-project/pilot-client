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
using System.Text;
using System.Text.RegularExpressions;
using XPilot.PilotClient.Common;

namespace XPilot.PilotClient.Network
{
    public enum FlightPlanType
    {
        IFR,
        VFR,
        DVFR,
        SVFR
    }

    [Serializable]
    public class FlightPlan
    {
        public string Callsign { get; set; }
        public FlightPlanType FlightType { get; set; }
        public string Equipment
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                bool flag = !string.IsNullOrEmpty(this.EquipmentPrefix);
                if (flag)
                {
                    stringBuilder.AppendFormat("{0}/", this.EquipmentPrefix);
                }
                stringBuilder.Append(this.AircraftType);
                bool flag2 = !string.IsNullOrEmpty(this.EquipmentSuffix);
                if (flag2)
                {
                    stringBuilder.AppendFormat("/{0}", this.EquipmentSuffix);
                }
                return stringBuilder.ToString();
            }
            set
            {
                this.ExtractEquipmentComponents(value);
            }
        }
        public int CruiseAltitude { get; set; }
        public int CruiseSpeed { get; set; }
        public string DepartureAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string AlternateAirport { get; set; }
        public string Route { get; set; }
        public string Remarks { get; set; }
        public bool IsHeavy { get; set; }
        public string EquipmentPrefix { get; set; }
        public string AircraftType { get; set; }
        public string EquipmentSuffix { get; set; }
        public int DepartureTime { get; set; }
        public int DepartureTimeAct { get; set; }
        public int EnrouteHours { get; set; }
        public int EnrouteMinutes { get; set; }
        public int FuelHours { get; set; }
        public int FuelMinutes { get; set; }
        public VoiceType VoiceType { get; set; }

        public string AircraftTypeWithSuffix
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(this.AircraftType);
                bool flag = !string.IsNullOrEmpty(this.EquipmentSuffix);
                if (flag)
                {
                    stringBuilder.AppendFormat("/{0}", this.EquipmentSuffix);
                }
                return stringBuilder.ToString();
            }
        }
        public FlightPlan()
        {
            this.FlightType = FlightPlanType.IFR;
            this.Equipment = "";
            this.CruiseAltitude = 0;
            this.CruiseSpeed = 0;
            this.DepartureAirport = "";
            this.DestinationAirport = "";
            this.AlternateAirport = "";
            this.Route = "";
            this.Remarks = "";
        }
        public FlightPlan(string callsign) : this()
        {
            this.Callsign = callsign;
        }
        public FlightPlan(FlightPlanType flightType, string equipment, int cruiseAlt, int cruiseSpd, string dep, string dest, string alt, string route, string remarks) : this()
        {
            this.FlightType = flightType;
            this.Equipment = equipment;
            this.CruiseAltitude = cruiseAlt;
            this.CruiseSpeed = cruiseSpd;
            this.DepartureAirport = dep;
            this.DestinationAirport = dest;
            this.AlternateAirport = alt;
            this.Route = route;
            this.Remarks = remarks;
        }
        internal void ExtractEquipmentComponents(string equipment)
        {
            Match match = Regex.Match(equipment, "^([A-Z]|\\d{1,2})/(\\S{2,4})/([A-Z])$", RegexOptions.IgnoreCase);
            bool success = match.Success;
            if (success)
            {
                this.EquipmentPrefix = match.Groups[1].Value;
                this.AircraftType = match.Groups[2].Value;
                this.EquipmentSuffix = match.Groups[3].Value;
            }
            else
            {
                match = Regex.Match(equipment, "^(\\S{2,4})/([A-Z])$", RegexOptions.IgnoreCase);
                bool success2 = match.Success;
                if (success2)
                {
                    this.EquipmentPrefix = string.Empty;
                    this.AircraftType = match.Groups[1].Value;
                    this.EquipmentSuffix = match.Groups[2].Value;
                }
                else
                {
                    match = Regex.Match(equipment, "^([A-Z]|\\d{1,2})/(\\S{2,4})$", RegexOptions.IgnoreCase);
                    bool success3 = match.Success;
                    if (success3)
                    {
                        this.EquipmentPrefix = match.Groups[1].Value;
                        this.AircraftType = match.Groups[2].Value;
                        this.EquipmentSuffix = string.Empty;
                    }
                    else
                    {
                        this.EquipmentPrefix = string.Empty;
                        this.AircraftType = equipment;
                        this.EquipmentSuffix = string.Empty;
                    }
                }
            }
        }
    }
}
