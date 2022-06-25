using GloboTicket.Integration.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.Integration.MessagingBus
{
    public class ConsoleMessageLog : IMessageBus
    {
        public Task PublishMessage(IntegrationBaseMessage message, string topicName)
        {
            Console.WriteLine("***************************");
            Console.WriteLine($"Topic: {topicName}");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine("***************************");

            return Task.FromResult<object>(null);
        }
    }
}