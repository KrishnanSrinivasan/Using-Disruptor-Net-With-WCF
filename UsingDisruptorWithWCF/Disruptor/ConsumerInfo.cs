namespace Disruptor
{
    internal class ConsumerInfo<T>
    {
        public ConsumerInfo(IBatchConsumer batchConsumer, IBatchHandler<T> handler)
        {
            BatchConsumer = batchConsumer;
            Handler = handler;
            IsEndOfChain = true;
        }

        public bool IsEndOfChain { get; private set; }
        public IBatchConsumer BatchConsumer { get; private set; }
        public IBatchHandler<T> Handler { get; private set; }

        public void UsedInBarrier()
        {
            IsEndOfChain = false;
        }
    }
}
