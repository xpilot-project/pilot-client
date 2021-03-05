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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vatsim.Xpilot.Aircrafts
{
    [Serializable]
    public class AircraftConfigurationInfo
    {
        [Serializable]
        public enum RequestType
        {
            [EnumMember(Value = "full")]
            Full
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("request")]
        public RequestType? Request { get; set; }
        [JsonProperty("config")]
        public AircraftConfiguration Config { get; set; }
        [JsonIgnore]
        public bool HasConfig { get { return Config != null; } }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        internal static AircraftConfigurationInfo FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AircraftConfigurationInfo>(json);
        }
    }

    [Serializable]
    public class AircraftConfiguration
    {
        [JsonProperty("is_full_data")]
        public bool? IsFullData { get; set; }
        [JsonIgnore]
        public bool IsIncremental { get { return !IsFullData.HasValue || !IsFullData.Value; } }
        [JsonProperty("lights")]
        public AircraftConfigurationLights Lights { get; set; }
        [JsonProperty("engines")]
        public AircraftConfigurationEngines Engines { get; set; }
        [JsonProperty("gear_down")]
        public bool? GearDown { get; set; }
        [JsonProperty("flaps_pct")]
        public int? FlapsPercent { get; set; }
        [JsonProperty("spoilers_out")]
        public bool? SpoilersDeployed { get; set; }
        [JsonProperty("on_ground")]
        public bool? OnGround { get; set; }

        internal void EnsurePopulated()
        {
            if (Lights == null) Lights = new AircraftConfigurationLights();
            Lights.EnsurePopulated();
            if (Engines == null) Engines = new AircraftConfigurationEngines();
            Engines.EnsurePopulated();
            if (!GearDown.HasValue) GearDown = true;
            if (!FlapsPercent.HasValue) FlapsPercent = 0;
            if (!SpoilersDeployed.HasValue) SpoilersDeployed = false;
            if (!OnGround.HasValue) OnGround = true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AircraftConfiguration other)) return false;
            if ((other.Lights == null) && (Lights != null)) return false;
            if ((other.Lights != null) && (Lights == null)) return false;
            if ((other.Engines == null) && (Engines != null)) return false;
            if ((other.Engines != null) && (Engines == null)) return false;
            return (
                ((other.Lights == null) || other.Lights.Equals(Lights))
                && ((other.Engines == null) || other.Engines.Equals(Engines))
                && other.GearDown == GearDown
                && other.FlapsPercent == FlapsPercent
                && other.SpoilersDeployed == SpoilersDeployed
                && other.OnGround == OnGround
            );
        }

        internal AircraftConfiguration Clone()
        {
            AircraftConfiguration clone = new AircraftConfiguration
            {
                IsFullData = IsFullData
            };
            if (Lights != null) clone.Lights = Lights.Clone();
            if (Engines != null) clone.Engines = Engines.Clone();
            clone.GearDown = GearDown;
            clone.FlapsPercent = FlapsPercent;
            clone.SpoilersDeployed = SpoilersDeployed;
            clone.OnGround = OnGround;
            return clone;
        }

        internal static AircraftConfiguration FromUserAircraftData(UserAircraftConfigData userAircraftConfigData)
        {
            AircraftConfiguration cfg = new AircraftConfiguration
            {
                Lights = AircraftConfigurationLights.FromUserAircraftData(userAircraftConfigData),
                Engines = AircraftConfigurationEngines.FromUserAircraftData(userAircraftConfigData),
                GearDown = userAircraftConfigData.GearDown,
                FlapsPercent = RoundUpToNearest5(userAircraftConfigData.FlapsRatio),
                SpoilersDeployed = userAircraftConfigData.SpoilersRatio != 0,
                OnGround = userAircraftConfigData.OnGround
            };
            return cfg;
        }

        private static int RoundUpToNearest5(double val)
        {
            return (int)(val * 100.0 / 5.0) * 5;
        }

        internal void ApplyIncremental(AircraftConfiguration inc)
        {
            if (inc.Lights != null) Lights.ApplyIncremental(inc.Lights);
            if (inc.Engines != null) Engines.ApplyIncremental(inc.Engines);
            if (inc.GearDown.HasValue) GearDown = inc.GearDown.Value;
            if (inc.FlapsPercent.HasValue) FlapsPercent = inc.FlapsPercent.Value;
            if (inc.SpoilersDeployed.HasValue) SpoilersDeployed = inc.SpoilersDeployed.Value;
            if (inc.OnGround.HasValue) OnGround = inc.OnGround.Value;
        }

        internal AircraftConfiguration CreateIncremental(AircraftConfiguration cfg)
        {
            EnsurePopulated();
            cfg.EnsurePopulated();
            AircraftConfiguration inc = new AircraftConfiguration
            {
                Lights = Lights.CreateIncremental(cfg.Lights),
                Engines = Engines.CreateIncremental(cfg.Engines)
            };
            if (cfg.GearDown != GearDown) inc.GearDown = cfg.GearDown;
            if (cfg.FlapsPercent != FlapsPercent) inc.FlapsPercent = cfg.FlapsPercent;
            if (cfg.SpoilersDeployed != SpoilersDeployed) inc.SpoilersDeployed = cfg.SpoilersDeployed;
            if (cfg.OnGround != OnGround) inc.OnGround = cfg.OnGround;
            return inc;
        }
    }

    [Serializable]
    public class AircraftConfigurationLights
    {
        [JsonProperty("strobe_on")]
        public bool? StrobesOn { get; set; }
        [JsonProperty("landing_on")]
        public bool? LandingOn { get; set; }
        [JsonProperty("taxi_on")]
        public bool? TaxiOn { get; set; }
        [JsonProperty("beacon_on")]
        public bool? BeaconOn { get; set; }
        [JsonProperty("nav_on")]
        public bool? NavOn { get; set; }

        internal void EnsurePopulated()
        {
            if (!StrobesOn.HasValue) StrobesOn = true;
            if (!LandingOn.HasValue) LandingOn = true;
            if (!TaxiOn.HasValue) TaxiOn = true;
            if (!BeaconOn.HasValue) BeaconOn = true;
            if (!NavOn.HasValue) NavOn = true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AircraftConfigurationLights acl)) return false;
            return (
                acl.StrobesOn == StrobesOn
                && acl.LandingOn == LandingOn
                && acl.TaxiOn == TaxiOn
                && acl.BeaconOn == BeaconOn
                && acl.NavOn == NavOn
            );
        }

        internal AircraftConfigurationLights Clone()
        {
            AircraftConfigurationLights clone = new AircraftConfigurationLights
            {
                StrobesOn = StrobesOn,
                LandingOn = LandingOn,
                TaxiOn = TaxiOn,
                BeaconOn = BeaconOn,
                NavOn = NavOn
            };
            return clone;
        }

        internal static AircraftConfigurationLights FromUserAircraftData(UserAircraftConfigData userAircraftConfigData)
        {
            AircraftConfigurationLights acl = new AircraftConfigurationLights
            {
                StrobesOn = userAircraftConfigData.StrobesOn,
                LandingOn = userAircraftConfigData.LandingLightsOn,
                TaxiOn = userAircraftConfigData.TaxiLightsOn,
                BeaconOn = userAircraftConfigData.BeaconOn,
                NavOn = userAircraftConfigData.NavLightOn
            };
            return acl;
        }

        internal void ApplyIncremental(AircraftConfigurationLights inc)
        {
            if (inc.StrobesOn.HasValue) StrobesOn = inc.StrobesOn.Value;
            if (inc.LandingOn.HasValue) LandingOn = inc.LandingOn.Value;
            if (inc.TaxiOn.HasValue) TaxiOn = inc.TaxiOn.Value;
            if (inc.BeaconOn.HasValue) BeaconOn = inc.BeaconOn.Value;
            if (inc.NavOn.HasValue) NavOn = inc.NavOn.Value;
        }

        internal AircraftConfigurationLights CreateIncremental(AircraftConfigurationLights cfg)
        {
            EnsurePopulated();
            cfg.EnsurePopulated();
            if (cfg.Equals(this)) return null;
            AircraftConfigurationLights inc = new AircraftConfigurationLights();
            if (cfg.StrobesOn != StrobesOn) inc.StrobesOn = cfg.StrobesOn;
            if (cfg.LandingOn != LandingOn) inc.LandingOn = cfg.LandingOn;
            if (cfg.TaxiOn != TaxiOn) inc.TaxiOn = cfg.TaxiOn;
            if (cfg.BeaconOn != BeaconOn) inc.BeaconOn = cfg.BeaconOn;
            if (cfg.NavOn != NavOn) inc.NavOn = cfg.NavOn;
            return inc;
        }
    }

    [Serializable]
    public class AircraftConfigurationEngines
    {
        [JsonProperty("1")]
        public AircraftConfigurationEngine Engine1 { get; set; }
        [JsonProperty("2")]
        public AircraftConfigurationEngine Engine2 { get; set; }
        [JsonProperty("3")]
        public AircraftConfigurationEngine Engine3 { get; set; }
        [JsonProperty("4")]
        public AircraftConfigurationEngine Engine4 { get; set; }
        [JsonIgnore]
        public bool HasEngine1Object { get { return Engine1 != null; } }
        [JsonIgnore]
        public bool HasEngine2Object { get { return Engine2 != null; } }
        [JsonIgnore]
        public bool HasEngine3Object { get { return Engine3 != null; } }
        [JsonIgnore]
        public bool HasEngine4Object { get { return Engine4 != null; } }

        [JsonIgnore]
        public bool IsAnyEngineRunning
        {
            get
            {
                return HasEngine1Object && Engine1.Running.Value
                    || HasEngine2Object && Engine2.Running.Value
                    || HasEngine3Object && Engine3.Running.Value
                    || HasEngine4Object && Engine4.Running.Value;
            }
        }

        internal void EnsurePopulated()
        {
            if (HasEngine1Object) Engine1.EnsurePopulated();
            if (HasEngine2Object) Engine2.EnsurePopulated();
            if (HasEngine3Object) Engine3.EnsurePopulated();
            if (HasEngine4Object) Engine4.EnsurePopulated();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AircraftConfigurationEngines other)) return false;
            if (!other.HasEngine1Object && HasEngine1Object) return false;
            if (other.HasEngine1Object && !HasEngine1Object) return false;
            if (!other.HasEngine2Object && HasEngine2Object) return false;
            if (other.HasEngine2Object && !HasEngine2Object) return false;
            if (!other.HasEngine3Object && HasEngine3Object) return false;
            if (other.HasEngine3Object && !HasEngine3Object) return false;
            if (!other.HasEngine4Object && HasEngine4Object) return false;
            if (other.HasEngine4Object && !HasEngine4Object) return false;
            return (
                (!other.HasEngine1Object || other.Engine1.Equals(Engine1))
                && (!other.HasEngine2Object || other.Engine2.Equals(Engine2))
                && (!other.HasEngine3Object || other.Engine3.Equals(Engine3))
                && (!other.HasEngine4Object || other.Engine4.Equals(Engine4))
            );
        }

        internal AircraftConfigurationEngines Clone()
        {
            AircraftConfigurationEngines clone = new AircraftConfigurationEngines();
            if (HasEngine1Object) clone.Engine1 = Engine1.Clone();
            if (HasEngine2Object) clone.Engine2 = Engine2.Clone();
            if (HasEngine3Object) clone.Engine3 = Engine3.Clone();
            if (HasEngine4Object) clone.Engine4 = Engine4.Clone();
            return clone;
        }

        internal static AircraftConfigurationEngines FromUserAircraftData(UserAircraftConfigData userAircraftConfigData)
        {
            AircraftConfigurationEngines ace = new AircraftConfigurationEngines();
            if (userAircraftConfigData.EngineCount >= 1) ace.Engine1 = AircraftConfigurationEngine.FromUserAircraftData(userAircraftConfigData, 1);
            if (userAircraftConfigData.EngineCount >= 2) ace.Engine2 = AircraftConfigurationEngine.FromUserAircraftData(userAircraftConfigData, 2);
            if (userAircraftConfigData.EngineCount >= 3) ace.Engine3 = AircraftConfigurationEngine.FromUserAircraftData(userAircraftConfigData, 3);
            if (userAircraftConfigData.EngineCount >= 4) ace.Engine4 = AircraftConfigurationEngine.FromUserAircraftData(userAircraftConfigData, 4);
            return ace;
        }

        internal void ApplyIncremental(AircraftConfigurationEngines inc)
        {
            if (inc.HasEngine1Object && HasEngine1Object) Engine1.ApplyIncremental(inc.Engine1);
            if (inc.HasEngine2Object && HasEngine2Object) Engine2.ApplyIncremental(inc.Engine2);
            if (inc.HasEngine3Object && HasEngine3Object) Engine3.ApplyIncremental(inc.Engine3);
            if (inc.HasEngine4Object && HasEngine4Object) Engine4.ApplyIncremental(inc.Engine4);
        }

        internal AircraftConfigurationEngines CreateIncremental(AircraftConfigurationEngines cfg)
        {
            EnsurePopulated();
            cfg.EnsurePopulated();
            if (cfg.Equals(this)) return null;
            AircraftConfigurationEngines inc = new AircraftConfigurationEngines();
            if (HasEngine1Object && cfg.HasEngine1Object) inc.Engine1 = Engine1.CreateIncremental(cfg.Engine1);
            if (HasEngine2Object && cfg.HasEngine2Object) inc.Engine2 = Engine2.CreateIncremental(cfg.Engine2);
            if (HasEngine3Object && cfg.HasEngine3Object) inc.Engine3 = Engine3.CreateIncremental(cfg.Engine3);
            if (HasEngine4Object && cfg.HasEngine4Object) inc.Engine4 = Engine4.CreateIncremental(cfg.Engine4);
            return inc;
        }
    }

    [Serializable]
    public class AircraftConfigurationEngine
    {
        [JsonProperty("on")]
        public bool? Running { get; set; }

        internal void EnsurePopulated()
        {
            if (!Running.HasValue) Running = true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AircraftConfigurationEngine ace)) return false;
            return ace.Running == Running;
        }

        internal AircraftConfigurationEngine Clone()
        {
            AircraftConfigurationEngine clone = new AircraftConfigurationEngine
            {
                Running = Running
            };
            return clone;
        }

        internal static AircraftConfigurationEngine FromUserAircraftData(UserAircraftConfigData userAircraftConfigData, int engineNum)
        {
            AircraftConfigurationEngine ace = new AircraftConfigurationEngine();
            switch (engineNum)
            {
                case 1: ace.Running = userAircraftConfigData.Engine1Running; break;
                case 2: ace.Running = userAircraftConfigData.Engine2Running; break;
                case 3: ace.Running = userAircraftConfigData.Engine3Running; break;
                case 4: ace.Running = userAircraftConfigData.Engine4Running; break;
            }
            return ace;
        }

        internal void ApplyIncremental(AircraftConfigurationEngine inc)
        {
            if (inc.Running.HasValue) Running = inc.Running;
        }

        internal AircraftConfigurationEngine CreateIncremental(AircraftConfigurationEngine cfg)
        {
            EnsurePopulated();
            cfg.EnsurePopulated();
            if (cfg.Equals(this)) return null;
            AircraftConfigurationEngine inc = new AircraftConfigurationEngine();
            if (cfg.Running != Running) inc.Running = cfg.Running;
            return inc;
        }
    }
}
