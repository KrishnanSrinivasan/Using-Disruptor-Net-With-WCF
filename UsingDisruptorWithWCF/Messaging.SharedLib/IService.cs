using System;
using System.ServiceModel;

namespace Messaging.SharedLib
{
    /// <summary>
    /// A Service that takes a byte stream to process. The Payload object is serialized and passed to this service.
    /// </summary>
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        void Process(Byte[] bytes);

        [OperationContract]
        String GetTotalMessageProcessedInfo();
    }
}
