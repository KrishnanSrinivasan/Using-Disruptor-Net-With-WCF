using System;

namespace Messaging.SharedLib
{
    /// <summary>
    /// Singleton Wrapper.
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct TSingleton<TType>
        where TType : new()
    {
        private static TType _instance = Activator.CreateInstance<TType>();

        public static TType Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
