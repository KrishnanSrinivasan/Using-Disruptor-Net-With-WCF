using System;
using System.ServiceModel;
using Messaging.SharedLib;

namespace Messaging.Consumer
{
    /// <summary>
    /// Simulates a Message Consumer.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Using LAMX Distruptor With WCF - Consumer");
            Console.WriteLine();
            
            TSingleton<MP1CSequencingBuffer<Byte[]>>.Instance.SetUp(
                Settings.Messaging.DefaultMessageStoreBufferSize, ByteArrayItemFactory.Create);

            TSingleton<MP1CSequencingBuffer<Byte[]>>.Instance.NewEntryAdded +=
                TSingleton<ByteArrayItemProcessor>.Instance.Process;

            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None);
            ServiceHost messageQueueHost = new ServiceHost(typeof(Service));
            messageQueueHost.AddServiceEndpoint(typeof(IService), netTcpBinding, Settings.Communincation.TCPURI);
            messageQueueHost.AddDefaultMEXEndPoint();
            messageQueueHost.EnableIncludeExceptionInFaultBehavior();
            messageQueueHost.Open();

            Console.WriteLine(messageQueueHost.GetHostedServiceEndPoints());
            Console.WriteLine("Service Host Running...");
            Console.WriteLine("Press <ENTER> to stop Service Host");
            Console.ReadLine();

            messageQueueHost.Close();

            TSingleton<MP1CSequencingBuffer<Byte[]>>.Instance.NewEntryAdded -=
                TSingleton<ByteArrayItemProcessor>.Instance.Process;

            TSingleton<MP1CSequencingBuffer<Byte[]>>.Instance.Dispose();
        }
    }
}
