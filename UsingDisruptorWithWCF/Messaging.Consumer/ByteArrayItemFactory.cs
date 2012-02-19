using System;

namespace Messaging.Consumer
{
    /// <summary>
    /// Factory to Create the ByteArrayItem to pre-allocate the MP1CSequencingBuffer
    /// </summary>
    struct ByteArrayItemFactory
    {
        const Int32 DEFAULT_PAYLOAD_SIZE = 1024;

        internal static ISequencingBufferEntry<Byte[]> Create() 
        {
            return new ByteArrayItem(DEFAULT_PAYLOAD_SIZE);
        }
    }
}