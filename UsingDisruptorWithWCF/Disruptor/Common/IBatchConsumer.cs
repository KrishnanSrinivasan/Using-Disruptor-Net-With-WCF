namespace Disruptor
{
    /// <summary>
    /// EntryConsumers waitFor entries to become available for consumption from the <see cref="RingBuffer{T}"/>
    /// </summary>
    public interface IBatchConsumer
    {
        /// <summary>
        /// Get the sequence up to which this Consumer has consumed entries
        /// Return the sequence of the last consumed entry
        /// </summary>
        long Sequence { get; }

        /// <summary>
        /// Return true if the instance is started, false otherwise
        /// </summary>
        bool Running { get; }

        /// <summary>
        /// Signal that this Consumer should stop when it has finished consuming at the next clean break.
        /// It will call <see cref="IConsumerBarrier{T}.Alert"/> to notify the thread to check status.
        /// </summary>
        void Halt();

        /// <summary>
        /// Starts the consumer 
        /// </summary>
        void Run();

        /// <summary>
        /// Throttle the sequence publication to other threads
        /// Can only be applied to the last consumers of a chain (the one tracked by the producer barrier)
        /// </summary>
        /// <param name="period">Sequence will be published every 'period' messages</param>
        void DelaySequenceWrite(int period);
    }
}