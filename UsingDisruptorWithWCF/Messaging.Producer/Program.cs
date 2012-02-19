using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using Messaging.SharedLib;

namespace Messaging.Producer
{
    /// <summary>
    /// Simulates Multiple Producers to send messages to a single consumer service.
    /// </summary>
    public class Program 
    {
        private static readonly NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None);
        private static readonly EndpointAddress remoteAddress = new EndpointAddress(Settings.Communincation.TCPURI);
        
        static void Main(string[] args)
        {
            Console.WriteLine("Using LAMX Distruptor With WCF - Producer");
            Console.WriteLine();
            Console.WriteLine("Sample Program to simulate message dispatch from {0} Producers.", Settings.Messaging.MaxProducers);
            Console.WriteLine("Press <ENTER> to continue.");
            Console.ReadLine();
            Console.WriteLine("Creating Producer Threads..");

            Thread[] producerThreads = new Thread[Settings.Messaging.MaxProducers];
            for (Int16 i = 0; i < Settings.Messaging.MaxProducers; i++)
            {
                producerThreads[i] = new Thread(SendMessage);
                producerThreads[i].Name = String.Format("Producer {0}", i);
                producerThreads[i].Start(i);
            }

            for (Int16 i = 0; i < Settings.Messaging.MaxProducers; i++)
            {
                producerThreads[i].Join();
            }

            Console.WriteLine("Message dispatch complete.");
            Console.WriteLine();
            Console.WriteLine("Total Messages Dispatched: {0}", Settings.Messaging.TotalMessageDispatched);
            Console.WriteLine(GetTotalMessageProcessedInfoFromService());
            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to exit.");
            Console.ReadLine();
        }

        private static String GetTotalMessageProcessedInfoFromService() 
        {
            IChannelFactory<IService> channelFatory = new ChannelFactory<IService>(netTcpBinding, remoteAddress);
            IService consumer = channelFatory.CreateChannel(remoteAddress);
            String totalMessageProcessedInfo = consumer.GetTotalMessageProcessedInfo();
            channelFatory.Close();
            return totalMessageProcessedInfo;
        }

        private static void SendMessage(object param) 
        {
            string producerName = Thread.CurrentThread.Name;
            Console.WriteLine("{0} Running...", producerName); 
            
            IChannelFactory<IService> channelFatory = new ChannelFactory<IService>(netTcpBinding, remoteAddress);
            IService consumer = channelFatory.CreateChannel(remoteAddress);

            ProducerInfo producer = new ProducerInfo((Int16)param, producerName);   
            
            for (Int64 i = 0; i < Settings.Messaging.MessagePerProducer; i++) 
            {
                Payload payload = new Payload(producer, i, String.Format("Hello World from {0}", producerName));
                Byte[] bytes = BinarySerializer.Serialize<Payload>(payload);
                consumer.Process(bytes);
            }
            
            channelFatory.Close();
        }
    }
}
