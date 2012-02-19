using System;

namespace Messaging.SharedLib
{
    /// <summary>
    /// Holds all the settings shared between Producer and Consumer.
    /// </summary>
    public struct Settings
    {
        public struct Messaging 
        {
            public const Int16 MaxProducers = 10;
            public const Int32 DefaultMessageStoreBufferSize = MaxProducers;
            public const Int64 MessagePerProducer = 1000;
            public const Int64 TotalMessageDispatched = MaxProducers * MessagePerProducer;
        }
        
        public struct Communincation 
        {
            public static Uri TCPURI = new Uri("net.tcp://localhost:1030/Service");
        }
    }
}
