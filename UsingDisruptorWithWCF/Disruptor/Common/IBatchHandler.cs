using System;

namespace Disruptor
{
    /// <summary>
    /// Callback interface to be implemented for processing <see cref="Entry{T}"/>s as they become available in the <see cref="RingBuffer{T}"/>
    /// </summary>
    /// <typeparam name="T">Data stored in the <see cref="Entry{T}"/> for sharing during exchange or parallel coordination of an event.</typeparam>
    public interface IBatchHandler<in T>
    {
        /// <summary>
        /// Called when a publisher has committed an <see cref="Entry{T}"/> to the <see cref="RingBuffer{T}"/>
        /// </summary>
        /// <param name="sequence">Sequence number committed to the <see cref="RingBuffer{T}"/></param>
        /// <param name="data">Data committed to the <see cref="RingBuffer{T}"/></param>
        /// <exception cref="Exception">If the BatchHandler would like the exception handled further up the chain.</exception>
        void OnAvailable(long sequence, T data);

        /// <summary>
        /// Called after each batch of items has been have been processed before the next waitFor call on a <see cref="IConsumerBarrier{T}"/>.
        /// This can be taken as a hint to do flush type operations before waiting once again on the <see cref="IConsumerBarrier{T}"/>.
        /// The user should not expect any pattern or frequency to the batch size.
        /// </summary>
        /// <exception cref="Exception">If the BatchHandler would like the exception handled further up the chain.</exception>
        void OnEndOfBatch();
    }
}