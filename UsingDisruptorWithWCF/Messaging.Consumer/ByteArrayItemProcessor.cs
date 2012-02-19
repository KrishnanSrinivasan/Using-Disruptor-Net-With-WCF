using System;
using System.Collections.Generic;
using System.Text;
using Messaging.SharedLib;

namespace Messaging.Consumer
{
    /// <summary>
    /// Simulates a simple business logic processor.It deserializes the incoming bytestream 
    /// into the PayLoad object and tracks the message count against each Producer.
    /// </summary>
    internal sealed class ByteArrayItemProcessor
    {
        internal Dictionary<ProducerInfo, Int64> _producerMessageCounter
             = new Dictionary<ProducerInfo, Int64>();

        internal void Process(ISequencingBufferEntry<Byte[]> byteArrayEntry) 
        {
            Payload payload = BinarySerializer.DeSerialize<Payload>(byteArrayEntry.Value);

            if (!_producerMessageCounter.ContainsKey(payload.Producer))
                _producerMessageCounter.Add(payload.Producer, 0);
                
            _producerMessageCounter[payload.Producer]++;
        }

        internal String GetProcessedMessageInfo() 
        {
            StringBuilder buidler = new StringBuilder();
            buidler.AppendLine("Processed Message Info:");

            Int64 totalMessageHandled = 0;
            foreach (ProducerInfo producer in _producerMessageCounter.Keys) 
            {
                
                buidler.AppendFormat("Message Handled For Producer[{0}]:{1}", producer, _producerMessageCounter[producer]);
                totalMessageHandled += _producerMessageCounter[producer];
                buidler.AppendLine();
            }

            buidler.AppendFormat("Total Message Handled:{0}", totalMessageHandled);
            return buidler.ToString();
        }
    }
}




