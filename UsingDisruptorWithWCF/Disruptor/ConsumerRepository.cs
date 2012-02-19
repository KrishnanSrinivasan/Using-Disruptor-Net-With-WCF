using System.Collections.Generic;
using System.Linq;

namespace Disruptor
{
    internal class ConsumerRepository<T>
    {
        private readonly IDictionary<IBatchHandler<T>, ConsumerInfo<T>> _consumerInfoByHandler = new Dictionary<IBatchHandler<T>, ConsumerInfo<T>>();
        private readonly IDictionary<IBatchConsumer, ConsumerInfo<T>> _consumerInfoByConsumer = new Dictionary<IBatchConsumer, ConsumerInfo<T>>();

        public void Add(IBatchConsumer batchConsumer, IBatchHandler<T> handler)
        {
            var consumerInfo = new ConsumerInfo<T>(batchConsumer, handler);
            _consumerInfoByHandler[handler] = consumerInfo;
            _consumerInfoByConsumer[batchConsumer] = consumerInfo;
        }

        public IBatchConsumer[] LastConsumersInChain
        {
            get
            {
                return (from consumerInfo in _consumerInfoByHandler.Values
                        where consumerInfo.IsEndOfChain
                        select consumerInfo.BatchConsumer).ToArray();
            }
        }
        
        public void UnmarkConsumersAsEndOfChain(IEnumerable<IBatchConsumer> barrierConsumers)
        {
            foreach (var barrierConsumer in barrierConsumers)
            {
                _consumerInfoByConsumer[barrierConsumer].UsedInBarrier();
            }
        }

        public IBatchConsumer GetConsumerFor(IBatchHandler<T> handler)
        {
            return _consumerInfoByHandler[handler].BatchConsumer;
        }

        public IEnumerable<ConsumerInfo<T>> Consumers
        {
            get { return _consumerInfoByHandler.Values; }
        }
    }
}
