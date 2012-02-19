namespace Disruptor
{
    /// <summary>
    /// Provides default values for <see cref="RingBuffer{T}"/> and associated classes.
    /// <see cref="RingBuffer{T}"/> is generic so it's easier to have a seperate class for that
    /// </summary>
    public static class RingBufferConvention
    {
        /// <summary>
        /// Initial sequence number used by <see cref="RingBuffer{T}"/> and associated classes
        /// </summary>
        public const long InitialCursorValue = -1L;
    }
}