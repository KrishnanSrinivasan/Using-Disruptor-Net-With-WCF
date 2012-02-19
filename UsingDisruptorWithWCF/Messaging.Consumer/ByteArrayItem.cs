using System;

namespace Messaging.Consumer
{
    /// <summary>
    /// A container for storing Byte[] value inside the MP1CSequencingBuffer.
    /// </summary>
    struct ByteArrayItem : ISequencingBufferEntry<Byte[]>
    {
        private Byte[] _internalStore;
        private Int64 _length;

        internal Int64 Length { get { return _length; } }

        internal ByteArrayItem(Int64 capacity)
        {
            _internalStore = new Byte[capacity];
            _length = capacity;
        }

        Byte[] ISequencingBufferEntry<byte[]>.Value
        {
            get { return _internalStore; }
        }

        void ISequencingBufferEntry<Byte[]>.CopyFrom(Byte[] source)
        {
            Array.Copy(source, _internalStore, source.Length);
            _length = source.Length;
        }
    }
}