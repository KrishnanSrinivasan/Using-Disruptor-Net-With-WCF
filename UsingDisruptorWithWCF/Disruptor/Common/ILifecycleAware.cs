namespace Disruptor
{
    /// <summary>
    /// Implement this interface to be notified when a thread for the <see cref="BatchConsumer{T}"/> starts and shuts down.
    /// </summary>
    public interface ILifecycleAware
    {
        ///<summary>
        /// Called once on thread start before first entry is available.
        ///</summary>
        void OnStart();

        /// <summary>
        /// Called once just before the thread is shutdown.
        /// </summary>
        void OnStop();
    }
}