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
    public class BinaryWriterEx : BinaryWriter
    {
        private BinaryWriterEx(Stream s) 
            : base(s) 
        {
        }

        public static BinaryWriterEx Create()
        {
            MemoryStream ms = new MemoryStream(1024);
            return new BinaryWriterEx(ms);
        }

        public override void Write(string str)
        {
            if (str == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                Write((byte)ObjType.stringType);
                base.Write(str);
            }
        }

        public void WriteByteArray(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        public override void Write(char[] c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int len = c.Length;
                Write(len);
                if (len > 0) base.Write(c);
            }
        }

        public void Write(DateTime dt) 
        { 
            Write(dt.Ticks); 
        }

        public void Write(Guid g)
        {
            Write((byte)ObjType.guidType);
            Byte[] b = g.ToByteArray();
            base.Write(b.Length);
            base.Write(b);
        }

        public void Write<T>(ICollection<T> c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (T item in c) PrivateWriteObject(item);
            }
        }

        public void Write<T, U>(IDictionary<T, U> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (KeyValuePair<T, U> kvp in d)
                {
                    PrivateWriteObject(kvp.Key);
                    PrivateWriteObject(kvp.Value);
                }
            }
        }

        public void WriteObject<T>(T obj)
            where T : class, ISerializable
        {
            new BinaryFormatter().Serialize(base.BaseStream, obj);
        }

        public void WriteEncoded(byte[] b) 
        {
            this.Write(Encoding.UTF8.GetString(b));
        }

        public void Flush(SerializationInfo info)
        {
            byte[] b = ToBinary();
            info.AddValue("X", b, typeof(byte[]));
            //base.BaseStream.Close();
        }

        private byte[] ToBinary()
        {
            return ((MemoryStream)base.BaseStream).ToArray();
        }

        private void PrivateWriteObject(object obj)
        {
            if (obj == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {

                switch (obj.GetType().Name)
                {

                    case "Boolean": Write((byte)ObjType.boolType);
                        Write((bool)obj);
                        break;

                    case "Byte": Write((byte)ObjType.byteType);
                        Write((byte)obj);
                        break;

                    case "UInt16": Write((byte)ObjType.uint16Type);
                        Write((ushort)obj);
                        break;

                    case "UInt32": Write((byte)ObjType.uint32Type);
                        Write((uint)obj);
                        break;

                    case "UInt64": Write((byte)ObjType.uint64Type);
                        Write((ulong)obj);
                        break;

                    case "SByte": Write((byte)ObjType.sbyteType);
                        Write((sbyte)obj);
                        break;

                    case "Int16": Write((byte)ObjType.int16Type);
                        Write((short)obj);
                        break;

                    case "Int32": Write((byte)ObjType.int32Type);
                        Write((int)obj);
                        break;

                    case "Int64": Write((byte)ObjType.int64Type);
                        Write((long)obj);
                        break;

                    case "Char": Write((byte)ObjType.charType);
                        base.Write((char)obj);
                        break;

                    case "String": Write((byte)ObjType.stringType);
                        base.Write((string)obj);
                        break;

                    case "Single": Write((byte)ObjType.singleType);
                        Write((float)obj);
                        break;

                    case "Double": Write((byte)ObjType.doubleType);
                        Write((double)obj);
                        break;

                    case "Decimal": Write((byte)ObjType.decimalType);
                        Write((decimal)obj);
                        break;

                    case "DateTime": Write((byte)ObjType.dateTimeType);
                        Write((DateTime)obj);
                        break;

                    case "Byte[]": Write((byte)ObjType.byteArrayType);
                        base.Write((byte[])obj);
                        break;

                    case "Char[]": Write((byte)ObjType.charArrayType);
                        base.Write((char[])obj);
                        break;

                    default: Write((byte)ObjType.otherType);
                        new BinaryFormatter().Serialize(BaseStream, obj);
                        break;
                }
            }
        }  
    }

    internal enum ObjType : byte
    {
        nullType,
        boolType,
        byteType,
        uint16Type,
        uint32Type,
        uint64Type,
        sbyteType,
        int16Type,
        int32Type,
        int64Type,
        charType,
        stringType,
        singleType,
        doubleType,
        decimalType,
        dateTimeType,
        byteArrayType,
        charArrayType,
        guidType,
        otherType
    }
}
