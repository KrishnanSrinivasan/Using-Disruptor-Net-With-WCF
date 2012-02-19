using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Messaging.SharedLib
{
    public struct BinarySerializer 
    {
        public static Byte[] Serialize<T>(T o)
            where T : class
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, o);
                return stream.ToArray();
            }
            catch { throw; }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }

        public static T DeSerialize<T>(Byte[] data)
            where T : class
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(data);
                stream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                object o = formatter.Deserialize(stream);
                return o as T;
            }
            catch { throw; }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}




