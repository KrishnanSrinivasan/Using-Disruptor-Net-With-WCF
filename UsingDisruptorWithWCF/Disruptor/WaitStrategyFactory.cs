using System;
using System.Threading;

namespace Disruptor
{
    /// <summary>
    /// Factory used by the <see cref="RingBuffer{T}"/> to instantiate the selected <see cref="IWaitStrategy"/>.
    /// </summary>
    public static class WaitStrategyFactory
    {
        /// <summary>
        /// Strategy options which are available to those waiting on a <see cref="RingBuffer{T}"/>
        /// </summary>
        public enum WaitStrategyOption
        {
            /// <summary>
            /// This strategy uses a condition variable inside a lock to block the consumer which saves CPU resource as the expense of lock contention.
            /// </summary>
            Blocking,
            /// <summary>
            /// This strategy calls Thread.yield() in a loop as a waiting strategy which reduces contention at the expense of CPU resource.
            /// </summary>
            Yielding,
            /// <summary>
            /// This strategy call spins in a loop as a waiting strategy which is lowest and most consistent latency but ties up a CPU
            /// </summary>
            BusySpin
        }

        /// <summary>
        /// Used by the <see cref="RingBuffer{T}"/> as a polymorphic constructor.
        /// </summary>
        /// <param name="option">Strategy type.</param>
        /// <returns>a new instance of the WaitStrategy</returns>
        public static IWaitStrategy GetInstance(WaitStrategyOption option)
        {
            switch (option)
            {
                case WaitStrategyOption.Blocking:
                    return new BlockingStrategy();
                case WaitStrategyOption.Yielding:
                    return new YieldingStrategy();
                case WaitStrategyOption.BusySpin:
                    return new BusySpinStrategy();
                    //TODO implement a strategy based on SpinWait
                default:
                    throw new ArgumentOutOfRangeException("option");
            }
        }

        /// <summary>
        /// Blocking strategy that uses a lock and condition variable for <see cref="IBatchConsumer"/>s waiting on a barrier.
        /// This strategy should be used when performance and low-latency are not as important as CPU resource.
        /// </summary>
        private sealed class BlockingStrategy : IWaitStrategy
        {
            private readonly object _gate = new object();

            public WaitForResult WaitFor<T>(IBatchConsumer[] consumers, ISequencable ringBuffer, IConsumerBarrier<T> barrier, long sequence)
            {
                var availableSequence = ringBuffer.Cursor; // volatile read
                if (availableSequence < sequence)
                {
                    lock(_gate)
                    {
                        while ((availableSequence = ringBuffer.Cursor) < sequence) // volatile read
                        {
                            if (barrier.IsAlerted)
                            {
                                return WaitForResult.AlertedResult;
                            }

                            Monitor.Wait(_gate);
                        }
                    }
                }

                if (0 != consumers.Length)
                {
                    while ((availableSequence = consumers.GetMinimumSequence()) < sequence)
                    {
                        if (barrier.IsAlerted)
                        {
                            return WaitForResult.AlertedResult;
                        }
                    }
                }

                return new WaitForResult(availableSequence, false);
            }

            public void SignalAll()
            {
                lock(_gate)
                {
                    Monitor.PulseAll(_gate);
                }
            }
        }

        /// <summary>
        /// Yielding strategy that uses a Thread.yield() for <see cref="IBatchConsumer"/>s waiting on a barrier.
        /// This strategy is a good compromise between performance and CPU resource.
        /// </summary>
        private sealed class YieldingStrategy:IWaitStrategy
        {
        	public WaitForResult WaitFor<T>(IBatchConsumer[] consumers, ISequencable ringBuffer, IConsumerBarrier<T> barrier, long sequence)
            {
                long availableSequence;

                if (0 == consumers.Length)
                {
                    while ((availableSequence = ringBuffer.Cursor) < sequence) // volatile read
                    {
                        if (barrier.IsAlerted)
                        {
                            return WaitForResult.AlertedResult;
                        }

                        Thread.Yield();
                    }
                }
                else
                {
                    while ((availableSequence = consumers.GetMinimumSequence()) < sequence)
                    {
                        if (barrier.IsAlerted)
                        {
                            return WaitForResult.AlertedResult;
                        }

                        Thread.Yield();
                    }
                }

                return new WaitForResult(availableSequence, false);
            }

            public void SignalAll()
            {
            }
        }

        /// <summary>
        /// Busy Spin strategy that uses a busy spin loop for <see cref="IBatchConsumer"/>s waiting on a barrier.
        /// This strategy will use CPU resource to avoid syscalls which can introduce latency jitter.  It is best
        /// used when threads can be bound to specific CPU cores.
        /// </summary>
        private sealed class BusySpinStrategy:IWaitStrategy
        {
            public WaitForResult WaitFor<T>(IBatchConsumer[] consumers, ISequencable ringBuffer, IConsumerBarrier<T> barrier, long sequence)
            {
                long availableSequence;

                if (0 == consumers.Length)
                {
                    while ((availableSequence = ringBuffer.Cursor) < sequence) // volatile read
                    {
                        if (barrier.IsAlerted)
                        {
                            return WaitForResult.AlertedResult;
                        }
                    }
                }
                else
                {
                    while ((availableSequence = consumers.GetMinimumSequence()) < sequence)
                    {
                        if (barrier.IsAlerted)
                        {
                            return WaitForResult.AlertedResult;
                        }
                    }
                }

                return new WaitForResult(availableSequence, false);
            }

            public void SignalAll()
            {
            }
        }
    }
}