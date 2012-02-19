namespace Disruptor
{
    /// <summary>
    /// Used to record the batch of sequences claimed in a <see cref="RingBuffer{T}"/>.
    /// </summary>
    public struct SequenceBatch
    {
        ///<summary>
        /// Create a holder for tracking a batch of claimed sequences in a <see cref="RingBuffer{T}"/>
        ///</summary>
        ///<param name="size">size of the batch</param>
        ///<param name="end"></param>
        internal SequenceBatch(int size, long end) : this()
        {
            Size = size;
            End = end;
            Start = end - (size - 1L);
        }

        /// <summary>
        /// Get the start sequence number of the batch.
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Get the size of the batch.
        /// </summary>
        public int Size { get; private set; }

        ///<summary>
        /// Get the end sequence number of the batch
        ///</summary>
        public long End { get; private set; }
    }
}