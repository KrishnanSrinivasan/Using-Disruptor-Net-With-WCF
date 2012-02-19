namespace Disruptor
{
    ///<summary>
    ///  A group of consumers set up via the <see cref="RingBuffer{T}"/>
    ///</summary>
    ///<typeparam name="T">the type of entry used by the consumers.</typeparam>
    public interface IConsumerGroup<T>
    {
        ///<summary>
        /// Set up batch handlers to consume events from the ring buffer. These handlers will only process events
        /// after every consumer in this group has processed the event.
        /// 
        /// This method is generally used as part of a chain. For example if the handler <code>A</code> must
        /// process events before handler <code>B</code>:
        /// <pre><code>dw.consumeWith(A).then(B);</code></pre>
        ///</summary>
        ///<param name="handlers">handlers the batch handlers that will consume events.</param>
        ///<returns>a <see cref="ConsumerGroup{T}"/> that can be used to set up a consumer barrier over the created consumers.</returns>
        IConsumerGroup<T> Then(params IBatchHandler<T>[] handlers);

        ///<summary>
        /// Set up batch handlers to consume events from the ring buffer. These handlers will only process events
        /// after every consumer in this group has processed the event.
        /// 
        /// <p>This method is generally used as part of a chain. For example if the handler <code>A</code> must
        /// process events before handler <code>B</code>:</p>
        /// 
        /// <pre><code>dw.after(A).consumeWith(B);</code></pre>
        ///</summary>
        ///<param name="handlers">handlers the batch handlers that will consume events.</param>
        ///<returns>a <see cref="ConsumerGroup{T}"/> that can be used to set up a consumer barrier over the created consumers.</returns>
        IConsumerGroup<T> ConsumeWith(IBatchHandler<T>[] handlers);
    }
}