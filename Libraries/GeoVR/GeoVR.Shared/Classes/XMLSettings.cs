using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace GeoVR.Shared
{
    public enum SettingsLocation
    {
        PublicDocuments,
        Local,
        Unknown
    }

    public static class XMLSettings
    {
        public static void Save<T>(this T objectToSerialize, string name, SettingsLocation location)
        {
            string file = GetLocationPath(name, location);
            using (var stream = new FileStream(file, FileMode.Create))
            {
                new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize);
            }
        }

        public static T Load<T>(string name, SettingsLocation location)
        {
            string file = GetLocationPath(name, location);
            using (var stream = new FileStream(file, FileMode.Open))
            {
                XmlSerializer s = new XmlSerializer(typeof(T));
                return (T)s.Deserialize(stream);
            }
        }

        public static SettingsLocation GetLocation(string name)
        {
            string file = GetLocationPath(name, SettingsLocation.Local);
            if (File.Exists(file))
                return SettingsLocation.Local;

            file = GetLocationPath(name, SettingsLocation.PublicDocuments);
            if (File.Exists(file))
                return SettingsLocation.PublicDocuments;

            return SettingsLocation.Unknown;
        }

        private static string GetLocationPath(string name, SettingsLocation location)
        {
            switch (location)
            {
                case SettingsLocation.PublicDocuments:
                    return Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), name + ".xml");
                case SettingsLocation.Local:
                    return Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), name + ".xml");
                default:
                    throw new Exception("SettingsLocation enum value not handled");
            }
        }
    }
}
