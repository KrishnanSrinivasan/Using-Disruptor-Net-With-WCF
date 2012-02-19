namespace Disruptor
{
    ///<summary>
    /// Implemented by RingBuffers and used by <see cref="IWaitStrategy"/>s to keep track of the current ring buffer sequence number
    ///</summary>
    public interface ISequencable
    {
        /// <summary>
        /// Get the current sequence that producers have committed to the RingBuffer.
        /// </summary>
        long Cursor { get; }
    }
}