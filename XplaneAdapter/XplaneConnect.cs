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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XPilot.PilotClient.XplaneAdapter
{
    [Serializable]
    public class XplaneConnect
    {
        [Serializable]
        public enum MessageType
        {
            [EnumMember(Value = "SetTransponderCode")]
            SetTransponderCode,
            [EnumMember(Value = "SetRadioFrequency")]
            SetRadioFrequency,
            [EnumMember(Value = "SetActiveComRadio")]
            SetActiveComRadio,
            [EnumMember(Value = "SetTransponderMode")]
            SetTransonderMode,
            [EnumMember(Value = "TransponderIdent")]
            TransponderIdent,
            [EnumMember(Value = "NetworkConnected")]
            NetworkConnected,
            [EnumMember(Value = "NetworkDisconnected")]
            NetworkDisconnected,
            [EnumMember(Value = "WhosOnline")]
            WhosOnline,
            [EnumMember(Value = "RequestAtis")]
            RequestAtis,
            [EnumMember(Value = "PluginVersion")]
            PluginVersion,
            [EnumMember(Value = "PluginHash")]
            PluginHash,
            [EnumMember(Value = "SocketMessage")]
            SocketMessage,
            [EnumMember(Value = "RadioMessage")]
            RadioMessage,
            [EnumMember(Value = "SimulatorMessage")]
            SimulatorMessage,
            [EnumMember(Value = "RemoveAllPlanes")]
            RemoveAllPlanes,
            [EnumMember(Value = "RemovePlane")]
            RemovePlane,
            [EnumMember(Value = "AddPlane")]
            AddPlane,
            [EnumMember(Value = "PositionUpdate")]
            PositionUpdate,
            [EnumMember(Value = "SurfaceUpdate")]
            SurfaceUpdate,
            [EnumMember(Value = "ChangeModel")]
            ChangeModel,
            [EnumMember(Value = "SetTransponder")]
            SetTransponder,
            [EnumMember(Value = "NetworkDisconnect")]
            NetworkDisconnect,
            [EnumMember(Value = "PluginDisabled")]
            PluginDisabled,
            [EnumMember(Value = "PrivateMessageReceived")]
            PrivateMessageReceived,
            [EnumMember(Value = "PrivateMessageSent")]
            PrivateMessageSent,
            [EnumMember(Value = "ValidateCslPaths")]
            ValidateCslPaths,
            [EnumMember(Value = "ForceDisconnect")]
            ForceDisconnect,
            [EnumMember(Value = "XplanePath")]
            XplanePath
        }

        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("Type")]
        public MessageType? Type { get; set; }

        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("Data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }
    }

    [Serializable]
    public class XplaneTextMessage
    {
        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("R")]
        public int Red { get; set; }

        [JsonProperty("G")]
        public int Green { get; set; }

        [JsonProperty("B")]
        public int Blue { get; set; }

        [JsonProperty("Direct")]
        public bool Direct { get; set; }
    }
}
