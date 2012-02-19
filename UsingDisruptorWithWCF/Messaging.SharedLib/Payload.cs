using System;
using System.Runtime.Serialization;

namespace Messaging.SharedLib
{
    /// <summary>
    /// Container for data passed from a Producer to a Consumer.
    /// </summary>
    [Serializable]
    public class Payload : ISerializable
    {
        public ProducerInfo Producer { get; private set; }
        public Int64 ID { get; private set; }
        public String Value { get; private set; }

        public Payload(ProducerInfo producer, Int64 ID, String value) 
        {
            this.Producer = producer;
            this.ID = ID;
            this.Value = value;
        }

        public Payload(SerializationInfo info, StreamingContext context)
        {
            BinaryReaderEx reader = BinaryReaderEx.Create(info);
            this.Producer = reader.ReadObject<ProducerInfo>();
            this.ID = reader.ReadInt16();
            this.Value = reader.ReadString();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            BinaryWriterEx writer = BinaryWriterEx.Create();
            writer.WriteObject<ProducerInfo>(this.Producer);
            writer.Write((Int16)this.ID);
            writer.Write(this.Value);
            writer.Flush(info);
        }
    }
}
