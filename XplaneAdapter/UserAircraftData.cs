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
    public class UserAircraftData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double AltitudeMsl { get; set; }
        public double PressureAltitude { get; set; }
        public double GroundSpeed { get; set; }
        public bool OnGround { get; set; }
        public double Flaps { get; set; }
        public bool SpeedBrakeDeployed { get; set; }
        public bool GearDown { get; set; }
        public bool StrobeLightsOn { get; set; }
        public bool BeaconLightsOn { get; set; }
        public bool TaxiLightsOn { get; set; }
        public bool LandingLightsOn { get; set; }
        public bool NavLightsOn { get; set; }
        public int TransponderCode { get; set; }
        public int TransponderMode { get; set; }
        public bool TransponderIdent { get; set; }
        public int EngineCount { get; set; }
        public bool Engine1Running { get; set; }
        public bool Engine2Running { get; set; }
        public bool Engine3Running { get; set; }
        public bool Engine4Running { get; set; }
        public bool ReplayModeEnabled { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(UserAircraftData other)
        {
            return Latitude == other.Latitude &&
                Longitude == other.Longitude &&
                Yaw == other.Yaw &&
                Roll == other.Roll &&
                Pitch == other.Pitch &&
                AltitudeMsl == other.AltitudeMsl &&
                PressureAltitude == other.PressureAltitude &&
                GroundSpeed == other.GroundSpeed &&
                OnGround == other.OnGround &&
                Flaps == other.Flaps &&
                SpeedBrakeDeployed == other.SpeedBrakeDeployed &&
                GearDown == other.GearDown &&
                StrobeLightsOn == other.StrobeLightsOn &&
                BeaconLightsOn == other.BeaconLightsOn &&
                TaxiLightsOn == other.TaxiLightsOn &&
                LandingLightsOn == other.LandingLightsOn &&
                NavLightsOn == other.NavLightsOn &&
                TransponderCode == other.TransponderCode &&
                TransponderMode == other.TransponderMode &&
                TransponderIdent == other.TransponderIdent &&
                EngineCount == other.EngineCount &&
                Engine1Running == other.Engine1Running &&
                Engine2Running == other.Engine2Running &&
                Engine3Running == other.Engine3Running &&
                Engine4Running == other.Engine4Running &&
                ReplayModeEnabled == other.ReplayModeEnabled;
        }
    }
}
