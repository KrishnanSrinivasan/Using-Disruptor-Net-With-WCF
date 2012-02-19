using System;

namespace Messaging.Consumer
{
    /// <summary>
    /// Interface for a Type that is stored in the MP1CSequencingBuffer. This interface allows the MP1CSequencingBuffer
    /// to directly copy only the value to an alreay pre-allocated ISequencingBufferEntry<T>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface ISequencingBufferEntry<T>
    {
        T Value { get; }
        void CopyFrom(T source);
    }
}