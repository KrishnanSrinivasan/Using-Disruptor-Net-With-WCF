namespace Disruptor
{
    /// <summary>
    /// Abstraction for claiming entries in a <see cref="RingBuffer{T}"/> while tracking dependent <see cref="IBatchConsumer"/>s.
    /// </summary>
    public interface IProducerBarrier<T> where T : class
    {
        ///<summary>
        /// Claim the next sequence number and a pre-allocated instance of T for a producer on the <see cref="RingBuffer{T}"/>
        ///</summary>
        ///<param name="data">A pre-allocated instance of T to be reused by the producer, to prevent memory allocation. This instance needs to be flushed properly before commiting back to the <see cref="RingBuffer{T}"/></param>
        ///<returns>the claimed sequence.</returns>
        long NextEntry(out T data);

        /// <summary>
        ///  Claim the next batch of entries in sequence.
        /// </summary>
        /// <param name="size">size of the batch</param>
        /// <returns>an instance of <see cref="SequenceBatch"/> containing the size and start sequence number of the batch</returns>
        SequenceBatch NextEntries(int size);

        /// <summary>
        /// Commit an entry back to the <see cref="RingBuffer{T}"/> to make it visible to <see cref="IBatchConsumer"/>s
        /// </summary>
        /// <param name="sequence">sequence number to be committed back to the <see cref="RingBuffer{T}"/></param>
        void Commit(long sequence);

        /// <summary>
        /// Commit a batch of entries to the <see cref="RingBuffer{T}"/> to make it visible to <see cref="IBatchConsumer"/>s.
        /// </summary>
        /// <param name="sequenceBatch"></param>
        void Commit(SequenceBatch sequenceBatch);

        /// <summary>
        /// Delegate a call to the <see cref="RingBuffer{T}.Cursor"/>
        /// </summary>
        long Cursor { get; }

        ///<summary>
        /// Get the data for a given sequence from the underlying <see cref="RingBuffer{T}"/>.
        ///</summary>
        ///<param name="sequence">sequence of the entry to get.</param>
        ///<returns>the data for the sequence</returns>
        T GetEntry(long sequence);
    }
}