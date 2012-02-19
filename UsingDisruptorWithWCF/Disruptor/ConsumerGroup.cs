namespace Disruptor
{
    ///<summary>
    ///  A group of consumers set up via the <see cref="RingBuffer{T}"/>
    ///</summary>
    ///<typeparam name="T">the type of entry used by the consumers.</typeparam>
    internal class ConsumerGroup<T> : IConsumerGroup<T>
    {
        private readonly IConsumerBuilder<T> _ringBuffer;
        private readonly IBatchConsumer[] _consumers;

        internal ConsumerGroup(IConsumerBuilder<T> ringBuffer, IBatchConsumer[] consumers)
        {
            _ringBuffer = ringBuffer;
            _consumers = consumers;
        }
        
        public IConsumerGroup<T> Then(params IBatchHandler<T>[] handlers)
        {
            return ConsumeWith(handlers);
        }

        public IConsumerGroup<T> ConsumeWith(IBatchHandler<T>[] handlers)
        {
            return _ringBuffer.CreateConsumers(_consumers, handlers);
        }
    }
}
