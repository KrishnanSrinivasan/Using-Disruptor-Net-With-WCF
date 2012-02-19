using System;
using System.Collections.Generic;
using System.Threading;
using Disruptor.MemoryLayout;

namespace Disruptor
{
    /// <summary>
    /// Ring based store of reusable entries containing the data representing an <see cref="Entry{T}"/> being exchanged between producers and consumers.
    /// </summary>
    /// <typeparam name="T">Entry implementation storing the data for sharing during exchange or parallel coordination of an event.</typeparam>
    public sealed class RingBuffer<T> : ISequencable, IConsumerBuilder<T> where T : class 
    {
        private readonly ClaimStrategyFactory.ClaimStrategyOption _claimStrategyOption;
        private CacheLineStorageLong _cursor = new CacheLineStorageLong(RingBufferConvention.InitialCursorValue);
        private readonly Entry<T>[] _entries;
        private readonly int _ringModMask;
        private readonly IClaimStrategy _claimStrategy;
        private readonly IWaitStrategy _waitStrategy;
        private readonly ConsumerRepository<T> _consumerRepository = new ConsumerRepository<T>();
        private readonly IList<Thread> _threads = new List<Thread>();
        private IProducerBarrier<T> _producerBarrier;

        /// <summary>
        /// Construct a RingBuffer with the full option set.
        /// </summary>
        /// <param name="entryFactory"> entryFactory to create instances of T for filling the RingBuffer</param>
        /// <param name="size">size of the RingBuffer that will be rounded up to the next power of 2</param>
        /// <param name="claimStrategyOption"> threading strategy for producers claiming entries in the ring.</param>
        /// <param name="waitStrategyOption">waiting strategy employed by consumers waiting on entries becoming available.</param>
        public RingBuffer(Func<T> entryFactory, int size, ClaimStrategyFactory.ClaimStrategyOption claimStrategyOption = ClaimStrategyFactory.ClaimStrategyOption.Multithreaded, WaitStrategyFactory.WaitStrategyOption waitStrategyOption = WaitStrategyFactory.WaitStrategyOption.Blocking)
        {
            _claimStrategyOption = claimStrategyOption;
            var sizeAsPowerOfTwo = Util.CeilingNextPowerOfTwo(size);
            _ringModMask = sizeAsPowerOfTwo - 1;
            _entries = new Entry<T>[sizeAsPowerOfTwo];

            _claimStrategy = ClaimStrategyFactory.GetInstance(claimStrategyOption);
            _waitStrategy = WaitStrategyFactory.GetInstance(waitStrategyOption);

            Fill(entryFactory);
        }

        /// <summary>
        /// The capacity of the RingBuffer to hold entries.
        /// </summary>
        public int Capacity
        {
            get { return _entries.Length; }
        }

        /// <summary>
        /// Get the current sequence that producers have committed to the RingBuffer.
        /// </summary>
        public long Cursor
        {
            get { return _cursor.Data; }
            private set{ _cursor.Data = value;}
        }

        ///<summary>
        /// Get the <see cref="Entry{T}"/> for a given sequence in the RingBuffer.
        ///</summary>
        ///<param name="sequence">sequence for the <see cref="Entry{T}"/></param>
        public Entry<T> this[long sequence]
        {
            get
            {
                return _entries[(int)sequence & _ringModMask];
            }
        }

        ///<summary>
        /// Set up batch handlers to consume events from the ring buffer. These handlers will process events
        /// as soon as they become available, in parallel.
        /// <p/>
        /// <p>This method can be used as the start of a chain. For example if the handler <code>A</code> must
        /// process events before handler <code>B</code>:</p>
        /// <p/>
        /// <pre><code>dw.consumeWith(A).then(B);</code></pre>
        ///</summary>
        ///<param name="handlers">handlers the batch handlers that will consume events.</param>
        ///<returns>a <see cref="IConsumerGroup{T}"/> that can be used to set up a consumer barrier over the created consumers.</returns>
        public IConsumerGroup<T> ConsumeWith(params IBatchHandler<T>[] handlers)
        {
            return ((IConsumerBuilder<T>) this).CreateConsumers(new IBatchConsumer[0], handlers);
        }

        ///<summary>
        /// Specifies a group of consumers that can then be used to build a barrier for dependent consumers.
        /// For example if the handler <code>A</code> must process events before handler <code>B</code>:
        /// <p/>
        /// <pre><code>dw.after(A).consumeWith(B);</code></pre>
        ///</summary>
        ///<param name="handlers">the batch handlers, previously set up with ConsumeWith,
        /// that will form the barrier for subsequent handlers.</param>
        ///<returns> a <see cref="IConsumerGroup{T}"/> that can be used to setup a consumer barrier over the specified consumers.</returns>
        public IConsumerGroup<T> After(params IBatchHandler<T>[] handlers)
        {
            var selectedConsumers = new IBatchConsumer[handlers.Length];
            for (int i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                selectedConsumers[i] = _consumerRepository.GetConsumerFor(handler);
                if (selectedConsumers[i] == null)
                {
                    throw new InvalidOperationException("Batch handlers must be consuming from the ring buffer before they can be used in a barrier condition.");
                }
            }
            return new ConsumerGroup<T>(this, selectedConsumers);
        }

        ///<summary>
        /// Create a producer barrier.  The barrier is set up to prevent overwriting any entry that is yet to
        /// be processed by a consumer that has already been set up.  As such, producer barriers should be
        /// created as the last step, after all handlers have been set up.
        ///</summary>
        ///<returns>the producer barrier.</returns>
        public IProducerBarrier<T> CreateProducerBarrier()
        {
            if (_producerBarrier == null)
            {
                var lastConsumersInChain = _consumerRepository.LastConsumersInChain;
                var period = _entries.Length / 2;
                foreach (var batchConsumer in lastConsumersInChain)
                {
                    batchConsumer.DelaySequenceWrite(period);
                }

                _producerBarrier = new ProducerBarrier(this, lastConsumersInChain);
            }

            return _producerBarrier;
        }

        /// <summary>
        ///  Create a <see cref="IConsumerBarrier{T}"/> that gates on the RingBuffer and a list of <see cref="IBatchConsumer"/>s
        /// </summary>
        /// <param name="consumersToTrack">consumersToTrack this barrier will track</param>
        /// <returns>the barrier gated as required</returns>
        public IConsumerBarrier<T> CreateConsumerBarrier(params IBatchConsumer[] consumersToTrack)
        {
            return new ConsumerBarrier<T>(this, consumersToTrack);
        }

        /// <summary>
        /// Create a <see cref="IProducerBarrier{T}"/> on this RingBuffer that tracks dependent <see cref="IBatchConsumer"/>s.
        /// </summary>
        /// <param name="consumersToTrack"></param>
        /// <returns></returns>
        public IProducerBarrier<T> CreateProducerBarrier(params IBatchConsumer[] consumersToTrack)
        {
            return new ProducerBarrier(this, consumersToTrack);
        }

        ///<summary>
        /// Calls <see cref="IBatchConsumer.Halt"/> on all the consumers
        ///</summary>
        public void Halt()
        {
            foreach (var consumerInfo in _consumerRepository.Consumers)
            {
                consumerInfo.BatchConsumer.Halt();
            }
            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        /// <summary>
        /// Start all consumer threads
        /// </summary>
        public void StartConsumers()
        {
            foreach (var consumerInfo in _consumerRepository.Consumers)
            {
                var thread = new Thread(consumerInfo.BatchConsumer.Run) { IsBackground = true };
                _threads.Add(thread);
                thread.Start();
            }

            //wait all BatchConsumers are properly started
            foreach (var consumerInfo in _consumerRepository.Consumers)
            {
                while (!consumerInfo.BatchConsumer.Running)
                {
                    // busy spin
                }
            }
        }

        ConsumerGroup<T> IConsumerBuilder<T>.CreateConsumers(IBatchConsumer[] barrierConsumers, IBatchHandler<T>[] batchHandlers)
        {
            if(_producerBarrier != null)
            {
                throw new InvalidOperationException("Producer Barrier must be initialised after all consumer barriers.");
            }

            var createdConsumers = new IBatchConsumer[batchHandlers.Length];
            for (int i = 0; i < batchHandlers.Length; i++)
            {
                var batchHandler = batchHandlers[i];
                var barrier = new ConsumerBarrier<T>(this, barrierConsumers);
                var batchConsumer = new BatchConsumer<T>(barrier, batchHandler);

                _consumerRepository.Add(batchConsumer, batchHandler);
                createdConsumers[i] = batchConsumer;
            }

            _consumerRepository.UnmarkConsumersAsEndOfChain(barrierConsumers);
            return new ConsumerGroup<T>(this, createdConsumers);
        }

        private void Fill(Func<T> entryFactory)
        {
            for (var i = 0; i < _entries.Length; i++)
            {
                var data = entryFactory();
                _entries[i] = new Entry<T>(-1, data);
            }
        }

        /// <summary>
        /// ConsumerBarrier handed out for gating consumers of the RingBuffer and dependent <see cref="IBatchConsumer"/>(s)
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        private sealed class ConsumerBarrier<TU> : IConsumerBarrier<TU> where TU : class
        {
            private readonly IBatchConsumer[] _consumers;
            private volatile bool _alerted;
            private readonly RingBuffer<TU> _ringBuffer;
            private readonly Entry<TU>[] _entries;
            private readonly int _ringModMask;
            private readonly IWaitStrategy _waitStrategy;

            public ConsumerBarrier(RingBuffer<TU> ringBuffer, params IBatchConsumer[] consumers)
            {
                _ringBuffer = ringBuffer;
                _consumers = consumers;
                _waitStrategy = _ringBuffer._waitStrategy;
                _ringModMask = _ringBuffer._ringModMask;
                _entries = _ringBuffer._entries;
            }

            public TU GetEntry(long sequence)
            {
                return _entries[(int)sequence & _ringModMask].Data;
            }

            public WaitForResult WaitFor(long sequence)
            {
                return _waitStrategy.WaitFor(_consumers, _ringBuffer, this, sequence);
            }

            public long Cursor
            {
                get { return _ringBuffer.Cursor; }
            }

            public bool IsAlerted
            {
                get { return _alerted; }
            }

            public void Alert()
            {
                _alerted = true;
                _waitStrategy.SignalAll();
            }

            public void ClearAlert()
            {
                _alerted = false;
            }
        }

        /// <summary>
        /// <see cref="IProducerBarrier{T}"/> that tracks multiple <see cref="IBatchConsumer"/>s when trying to claim
        /// a <see cref="Entry{T}"/> in the <see cref="RingBuffer{T}"/>.
        /// </summary>
        private sealed class ProducerBarrier  : IProducerBarrier<T> 
        {
            private readonly RingBuffer<T> _ringBuffer;
            private readonly IBatchConsumer[] _consumers;
            private readonly Entry<T>[] _entries;
            private readonly IClaimStrategy _claimStrategy;
            private readonly int _ringModMask;
            private readonly int _ringBufferSize;
            private readonly bool _isMultithreaded;
            private long _lastConsumerMinimum = RingBufferConvention.InitialCursorValue;
            private readonly IWaitStrategy _waitStrategy;

            public ProducerBarrier(RingBuffer<T> ringBuffer, params IBatchConsumer[] consumers)
            {
                if (consumers.Length == 0)
                {
                    throw new ArgumentException("There must be at least one Consumer to track for preventing ring wrap");
                }

                _ringBuffer = ringBuffer;
                _consumers = consumers;
                _entries = _ringBuffer._entries;
                _claimStrategy = _ringBuffer._claimStrategy;
                _ringModMask = _ringBuffer._ringModMask;
                _ringBufferSize = _entries.Length;
                _waitStrategy = _ringBuffer._waitStrategy;
                _isMultithreaded = _ringBuffer._claimStrategyOption == ClaimStrategyFactory.ClaimStrategyOption.Multithreaded;
            }

            public long NextEntry(out T data)
            {
                var sequence = _claimStrategy.IncrementAndGet();
                EnsureConsumersAreInRange(sequence);

                data = _entries[(int) sequence & _ringModMask].Data;

                return sequence;
            }

            public SequenceBatch NextEntries(int size)
            {
                long sequence = _claimStrategy.IncrementAndGet(size);
                var sequenceBatch = new SequenceBatch(size, sequence);
                EnsureConsumersAreInRange(sequence);

                return sequenceBatch;
            }

            public void Commit(long sequence)
            {
                Commit(sequence, 1L);
            }

            public void Commit(SequenceBatch sequenceBatch)
            {
                Commit(sequenceBatch.End, sequenceBatch.Size);
            }

            public long Cursor
            {
                get { return _ringBuffer.Cursor; }
            }

            public T GetEntry(long sequence)
            {
                return _entries[(int)sequence & _ringModMask].Data;
            }

            private void EnsureConsumersAreInRange(long sequence)
            {
                var wrapPoint = sequence - _ringBufferSize;
                
                while (wrapPoint > _lastConsumerMinimum && wrapPoint > (_lastConsumerMinimum = _consumers.GetMinimumSequence()))
                {
                    Thread.Yield();
                }
            }

            private void Commit(long sequence, long batchSize)
            {
                if (_isMultithreaded)
                {
                    long expectedSequence = sequence - batchSize;
                    while (expectedSequence != _ringBuffer.Cursor)
                    {
                        // busy spin
                    }
                }

                _ringBuffer.Cursor = sequence; // volatile write
                _waitStrategy.SignalAll();
            }
        }
    }
}