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
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace XPilot.PilotClient.Common
{
    public static class SerializationUtils
    {
        public static void SerializeObjectToFile<T>(T obj, string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                xmlSerializer.Serialize(streamWriter, obj);
            }
        }

        public static T DeserializeObjectFromFile<T>(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T result;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                T t = (T)((object)xmlSerializer.Deserialize(new XmlTextReader(fileStream)
                {
                    Normalization = false
                }));
                bool flag = t is IDeserializationCallback;
                if (flag)
                {
                    (t as IDeserializationCallback).OnDeserialize();
                }
                result = t;
            }
            return result;
        }
    }

    public interface IDeserializationCallback
    {
        void OnDeserialize();
    }
}
