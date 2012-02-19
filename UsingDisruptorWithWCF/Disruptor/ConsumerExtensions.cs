namespace Disruptor
{
    /// <summary>
    /// Provides an extension method for <see cref="IBatchConsumer"/>[]
    /// </summary>
    public static class ConsumerExtensions
    {
        /// <summary>
        /// Get the minimum sequence from an array of <see cref="IBatchConsumer"/>s.
        /// </summary>
        /// <param name="consumers">consumers to compare.</param>
        /// <returns>the minimum sequence found or lon.MaxValue if the array is empty.</returns>
        public static long GetMinimumSequence(this IBatchConsumer[] consumers)
        {
            if (consumers.Length == 0) return long.MaxValue;

            var min = long.MaxValue;
            for (var i = 0; i < consumers.Length; i++)
            {
                var sequence = consumers[i].Sequence; // volatile read
                min = min < sequence ? min : sequence;
            }
            return min;
        }
    }
}