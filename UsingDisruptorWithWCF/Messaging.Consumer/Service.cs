using System;
using Messaging.SharedLib;

namespace Messaging.Consumer
{
    /// <summary>
    /// A Service that reads byte data from a underlying transport layer and passes it
    /// to the SequencingBuffer to be futher passed on to the Business Logic Processor.
    /// </summary>
    internal sealed class Service : IService
    {
        void IService.Process(byte[] bytes)
        {
            TSingleton<MP1CSequencingBuffer<Byte[]>>.Instance.CopyToStoreEntry(bytes);
        }

        String IService.GetTotalMessageProcessedInfo()
        {
            return TSingleton<ByteArrayItemProcessor>.Instance.GetProcessedMessageInfo();
        }
    }
}
