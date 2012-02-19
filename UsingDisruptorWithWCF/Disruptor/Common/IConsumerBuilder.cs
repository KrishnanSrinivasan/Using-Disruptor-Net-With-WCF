namespace Disruptor
{
    internal interface IConsumerBuilder<T>
    {
        ConsumerGroup<T> CreateConsumers(IBatchConsumer[] barrierConsumers, IBatchHandler<T>[] batchHandlers);
    }
}