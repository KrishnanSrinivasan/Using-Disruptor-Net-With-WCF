using Disruptor.MemoryLayout;

namespace Disruptor
{
    /// <summary>
    /// Convenience class for handling the batching semantics of consuming entries from a <see cref="RingBuffer{T}"/>
    /// and delegating the available <see cref="Entry{T}"/>s to a <see cref="IBatchHandler{T}"/>.
    /// 
    /// If the {@link BatchHandler} also implements {@link LifecycleAware} it will be notified just after the thread
    /// is started and just before the thread is shutdown.
    /// </summary>
    /// <typeparam name="T">Entry implementation storing the data for sharing during exchange or parallel coordination of an event.</typeparam>
    internal sealed class BatchConsumer<T> : IBatchConsumer
    {
        private readonly IConsumerBarrier<T> _consumerBarrier;
        private readonly IBatchHandler<T> _handler;

        private CacheLineStorageBool _running = new CacheLineStorageBool(true);
        private CacheLineStorageLong _sequence = new CacheLineStorageLong(RingBufferConvention.InitialCursorValue);
        private bool _delaySequenceWrite;
        private int _sequenceUpdatePeriod;
        private int _nextSequencePublish = 1;

        /// <summary>
        /// Construct a batch consumer that will automatically track the progress by updating its sequence when
        /// the <see cref="IBatchHandler{T}.OnAvailable"/> method returns.
        /// </summary>
        /// <param name="consumerBarrier">consumerBarrier on which it is waiting.</param>
        /// <param name="handler">handler is the delegate to which <see cref="Entry{T}"/>s are dispatched.</param>
        public BatchConsumer(IConsumerBarrier<T> consumerBarrier, IBatchHandler<T> handler)
        {
            _consumerBarrier = consumerBarrier;
            _handler = handler;
        }

        /// <summary>
        /// Throttle the sequence publication to other threads
        /// Can only be applied to the last consumers of a chain (the one tracked by the producer barrier)
        /// </summary>
        /// <param name="period">Sequence will be published every 'period' messages</param>
        public void DelaySequenceWrite(int period)
        {
            _delaySequenceWrite = true;
            _sequenceUpdatePeriod = period;
            _nextSequencePublish = period;
        }

        /// <summary>
        /// Get the <see cref="IConsumerBarrier{T}"/> the <see cref="IBatchConsumer"/> is waiting on.
        /// </summary>
        public IConsumerBarrier<T> ConsumerBarrier
        {
            get { return _consumerBarrier; }
        }

        /// <summary>
        /// Return true if the instance is started, false otherwise
        /// </summary>
        public bool Running
        {
            get { return _running.Data; }
        }

        /// <summary>
        /// It is ok to have another thread rerun this method after a halt().
        /// </summary>
        public void Run()
        {
            _running.Data = true;
            
            OnStart();

            var nextSequence = Sequence + 1; 

            while (_running.Data)
            {
                var waitForResult = _consumerBarrier.WaitFor(nextSequence);
                if (!waitForResult.IsAlerted)
                {
                    var availableSequence = waitForResult.AvailableSequence;
                    while (nextSequence <= availableSequence)
                    {
                        T data = _consumerBarrier.GetEntry(nextSequence);
                        _handler.OnAvailable(nextSequence, data);
                        
                        nextSequence++;
                    }
                    _handler.OnEndOfBatch();

                    if(_delaySequenceWrite)
                    {
                        if(nextSequence > _nextSequencePublish)
                        {
                            Sequence = nextSequence - 1; // volatile write
                            _nextSequencePublish += _sequenceUpdatePeriod;
                        }
                    }
                    else
                    {
                        Sequence = nextSequence - 1; // volatile write
                    }
                }
            }

            OnStop();
        }

        private void OnStop()
        {
            var lifecycleAware = _handler as ILifecycleAware;
            if (lifecycleAware != null)
            {
                lifecycleAware.OnStop();
            }
        }

        private void OnStart()
        {
            var lifecycleAware = _handler as ILifecycleAware;
            if(lifecycleAware != null)
            {
                lifecycleAware.OnStart();
            }
        }

        /// <summary>
        /// Get the sequence up to which this Consumer has consumed <see cref="Entry{T}"/>s
        /// Return the sequence of the last consumed <see cref="Entry{T}"/>
        /// </summary>
        public long Sequence
        {
            get { return _sequence.Data; }
            private set { _sequence.Data = value;}
        }

        /// <summary>
        /// Signal that this Consumer should stop when it has finished consuming at the next clean break.
        /// It will call <see cref="IConsumerBarrier{T}.Alert"/> to notify the thread to check status.
        /// </summary>
        public void Halt()
        {
            _running.Data = false;
            _consumerBarrier.Alert();
        }
    }
}