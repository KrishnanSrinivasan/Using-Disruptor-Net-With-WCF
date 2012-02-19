using System;

namespace Disruptor
{
    /// <summary>
    /// No operation version of a <see cref="IBatchConsumer"/> that simply tracks a <see cref="RingBuffer{T}"/>.
    ///  This is useful in tests or for pre-filling a <see cref="RingBuffer{T}"/> from a producer.
    /// </summary>
    public sealed class NoOpConsumer<T>:IBatchConsumer where T : class
    {
        private readonly ISequencable _ringBuffer;
        private volatile bool _running;

        /// <summary>
        /// Construct a <see cref="IBatchConsumer"/> that simply tracks a <see cref="RingBuffer{T}"/>.
        /// </summary>
        /// <param name="ringBuffer"></param>
        public NoOpConsumer(ISequencable ringBuffer)
        {
            _ringBuffer = ringBuffer;
        }

        /// <summary>
        /// NoOp
        /// </summary>
        public void Run()
        {
            _running = true;
        }

        /// <summary>
        /// No op
        /// </summary>
        /// <param name="period"></param>
        public void DelaySequenceWrite(int period)
        {
            
        }

        /// <summary>
        /// Delegates call to <see cref="RingBuffer{T}.Cursor"/>
        /// </summary>
        public long Sequence
        {
            get { return _ringBuffer.Cursor; }
        }

        /// <summary>
        /// Return true if the instance is started, false otherwise
        /// </summary>
        public bool Running
        {
            get { return _running; }
        }

        /// <summary>
        /// NoOp
        /// </summary>
        public void Halt()
        {
            _running = false;
        }
    }
}