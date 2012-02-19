using System;
using System.Runtime.Serialization;

namespace Messaging.SharedLib
{
    /// <summary>
    /// Container to hold Producer information. 
    /// </summary>
    [Serializable]
    public class ProducerInfo : ISerializable
    {
        public Int16 ID { get; private set; }
        public String Name { get; private set; }

        public ProducerInfo(Int16 ID, String name) 
        {
            this.ID = ID;
            this.Name = name;
        }

        public ProducerInfo(SerializationInfo info, StreamingContext context)
        {
            BinaryReaderEx reader = BinaryReaderEx.Create(info);
            this.ID = reader.ReadInt16();
            this.Name = reader.ReadString();
        }

        public override int GetHashCode()
        {
            return this.ID;
        }

        public override bool Equals(object obj)
        {
            ProducerInfo o = obj as ProducerInfo;
            if (o == null) return false;

            return o.ID == this.ID;
        }

        public override string ToString()
        {
            return this.Name;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            BinaryWriterEx writer = BinaryWriterEx.Create();
            writer.Write((Int16)this.ID);
            writer.Write(this.Name);
            writer.Flush(info);
        }
    }
}
