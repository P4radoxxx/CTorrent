using System;
using System.IO;
using System.Xml.Serialization;

namespace Client_torrent
{
    public static class Serialization
    {
        public static bool Serialiser<T>(T content, string path)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xs.Serialize(wr, content);
                }
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static T Deserialiser<T>(string path)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (StreamReader rd = new StreamReader(path))
                {
                    return (T)xs.Deserialize(rd);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return default(T);
            }
        }

    }
}
