using System;
using Disruptor;

namespace Messaging.Consumer
{
    /// <summary>
    /// A simple wrapper around the RingBuffer v1.0, illustrating the usage of the 
    /// Ring Buffer for a Multi-Producer(MP), Single-Consumer(1C) Scenario. 
    /// </summary>
    /// <typeparam name="TValue">Type wrapped by the ISequencingBufferEntry to facilicate xcopy from a source.</typeparam>
    internal sealed class MP1CSequencingBuffer<TValue> : IDisposable
    {
        private RingBuffer<ISequencingBufferEntry<TValue>> _ringBuffer;
        private IProducerBarrier<ISequencingBufferEntry<TValue>> _producerBarrier;

        internal event Action<ISequencingBufferEntry<TValue>> NewEntryAdded = delegate { };

        internal void SetUp(Int32 size, Func<ISequencingBufferEntry<TValue>> messageStoreEntryFactory)
        {
            _ringBuffer = new RingBuffer<ISequencingBufferEntry<TValue>>(
                                messageStoreEntryFactory,
                                size,
                                ClaimStrategyFactory.ClaimStrategyOption.Multithreaded,
                                WaitStrategyFactory.WaitStrategyOption.Yielding);

            SequentialMessageDispatch<ISequencingBufferEntry<TValue>> sequentialDispatch =
                new SequentialMessageDispatch<ISequencingBufferEntry<TValue>>(
                    (ISequencingBufferEntry<TValue> entry) => 
                    {
                        NewEntryAdded(entry);
                    });

            _ringBuffer.ConsumeWith(sequentialDispatch);
            _producerBarrier = _ringBuffer.CreateProducerBarrier();

            _ringBuffer.StartConsumers();
        }

        internal void CopyToStoreEntry(TValue value) 
        {
            ISequencingBufferEntry<TValue> storeEntry;
            var sequence = _producerBarrier.NextEntry(out storeEntry);
            storeEntry.CopyFrom(value);
            _producerBarrier.Commit(sequence);
        }

        public void Dispose()
        {
            _ringBuffer.Halt();
        }
    }
}
