namespace Disruptor
{
    /// <summary>
    /// Coordination barrier for tracking the cursor for producers and sequence of
    /// dependent <see cref="IBatchConsumer"/>s for a <see cref="RingBuffer{T}"/>
    /// </summary>
    public interface IConsumerBarrier<out T>
    {
        /// <summary>
        /// Get the data for a given sequence from the underlying <see cref="RingBuffer{T}"/>.
        /// </summary>
        /// <param name="sequence">sequence of the entry to get.</param>
        /// <returns>the data for the sequence.</returns>
        T GetEntry(long sequence);

        /// <summary>
        /// Wait for the given sequence to be available for consumption.
        /// </summary>
        /// <param name="sequence">sequence to wait for</param>
        /// <returns>the sequence up to which is available</returns>
        WaitForResult WaitFor(long sequence);

        /// <summary>
        /// Delegate a call to the <see cref="RingBuffer{T}.Cursor"/>
        /// Returns the value of the cursor for entries that have been published.
        /// </summary>
        long Cursor { get; }

        /// <summary>
        /// The current alert status for the barrier.
        /// Returns true if in alert otherwise false.
        /// </summary>
        bool IsAlerted { get; }

        /// <summary>
        ///  Alert the consumers of a status change and stay in this status until cleared.
        /// </summary>
        void Alert();

        /// <summary>
        /// Clear the current alert status.
        /// </summary>
        void ClearAlert();
    }
}

