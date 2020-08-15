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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XPilot.PilotClient.Config
{
    public class StrictIntConverter : JsonConverter
    {
        readonly JsonSerializer defaultSerializer = new JsonSerializer();

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsIntegerType();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float: // Accepts numbers like 4.00
                case JsonToken.Null:
                    return defaultSerializer.Deserialize(reader, objectType);
                default:
                    throw new JsonSerializationException(string.Format("Token \"{0}\" of type {1} was not a JSON integer", reader.Value, reader.TokenType));
            }
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public static class JsonExtensions
    {
        public static bool IsIntegerType(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(System.Numerics.BigInteger))
                return true;
            return false;
        }
    }
}
