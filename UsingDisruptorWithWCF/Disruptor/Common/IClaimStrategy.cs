namespace Disruptor
{
    /// <summary>
    /// Strategies employed for claiming the sequence of <see cref="Entry{T}"/>s in the <see cref="RingBuffer{T}"/> by producers.
    /// </summary>
    public interface IClaimStrategy
    {
        /// <summary>
        /// Increment the sequence index in the <see cref="RingBuffer{T}"/> and return the new value
        /// </summary>
        /// <returns>The <see cref="Entry{T}"/> index to be used for the producer.</returns>
        long IncrementAndGet();

        ///<summary>
        /// Increment by a delta and get the result.
        ///</summary>
        ///<param name="delta">delta to increment by.</param>
        ///<returns>the result after incrementing.</returns>
        long IncrementAndGet(int delta);
    }
}
