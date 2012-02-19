using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Messaging.SharedLib
{
    /// <summary>
    /// Base Code taken from http://www.codeproject.com/Articles/14164/A-Fast-Serialization-Technique 
    /// and futher improvised.
    /// </summary>
    public class BinaryReaderEx : BinaryReader, IDisposable
    {
        private BinaryReaderEx(Stream s) 
            : base(s) 
        {
        }

        public static BinaryReaderEx Create(SerializationInfo info)
        {
            byte[] byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            return GetReaderEx(byteArray);
        }

        private static BinaryReaderEx GetReaderEx(byte[] buffer)
        {
            return new BinaryReaderEx(new MemoryStream(buffer));
        }

        public override string ReadString()
        {
            ObjType t = (ObjType)ReadByte();
            if (t == ObjType.stringType) return base.ReadString();
            return null;
        }

        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        public byte[] ReadEndcodedByteArray() 
        {
            return Encoding.UTF8.GetBytes(this.ReadString());
        }

        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;
            return new char[0];
        }

        public DateTime ReadDateTime() 
        { 
            return new DateTime(ReadInt64()); 
        }

        public Guid ReadGuid()
        {
            ObjType t = (ObjType)ReadByte();
            if (t != ObjType.guidType) return Guid.Empty;
            int len = ReadInt32();
            Byte[] b = this.ReadBytes(len);
            return new Guid(b);
        }

        public IList<T> ReadList<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (int i = 0; i < count; i++) d.Add((T)PrivateReadObject());
            return d;
        }

        public IDictionary<T, U> ReadDictionary<T, U>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary<T, U> d = new Dictionary<T, U>();
            for (int i = 0; i < count; i++) d[(T)PrivateReadObject()] = (U)PrivateReadObject();
            return d;
        }

        public T ReadObject<T>() 
            where T:class, ISerializable
        {
            return new BinaryFormatter().Deserialize(base.BaseStream) as T;
        }

        private object PrivateReadObject()
        {
            ObjType t = (ObjType)ReadByte();
            switch (t)
            {
                case ObjType.boolType: return base.ReadBoolean();
                case ObjType.byteType: return base.ReadByte();
                case ObjType.uint16Type: return base.ReadUInt16();
                case ObjType.uint32Type: return base.ReadUInt32();
                case ObjType.uint64Type: return base.ReadUInt64();
                case ObjType.sbyteType: return base.ReadSByte();
                case ObjType.int16Type: return base.ReadInt16();
                case ObjType.int32Type: return base.ReadInt32();
                case ObjType.int64Type: return base.ReadInt64();
                case ObjType.charType: return base.ReadChar();
                case ObjType.stringType: return base.ReadString();
                case ObjType.singleType: return base.ReadSingle();
                case ObjType.doubleType: return base.ReadDouble();
                case ObjType.decimalType: return base.ReadDecimal();
                case ObjType.dateTimeType: return this.ReadDateTime();
                case ObjType.byteArrayType: return this.ReadByteArray();
                case ObjType.charArrayType: return this.ReadCharArray();
                case ObjType.otherType: return new BinaryFormatter().Deserialize(BaseStream);
                default: return null;
            }
        }
        
        void IDisposable.Dispose()
        {
            if (base.BaseStream != null)
                base.BaseStream.Close();
        }
    }
}
