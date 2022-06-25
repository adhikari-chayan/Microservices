using GloboTicket.Integration.Helpers;
using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Ordering.Messages;
using GloboTicket.Services.Ordering.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.Services.Ordering.Messaging
{
    public class PaymentUpdateConsumer : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly OrderRepository orderRepository;
        private readonly IMessageBus messageBus;

        private string orderPaymentUpdatedMessageQueue;
        private string hostName;
        private int port;

        private IConnection connection;
        private IModel channel;
        private ILogger<PaymentUpdateConsumer> logger;

        public PaymentUpdateConsumer(IConfiguration configuration, IMessageBus messageBus, OrderRepository orderRepository)
        {
            this.configuration = configuration;
            this.orderRepository = orderRepository;
            this.messageBus = messageBus;

            getConfiguration();
            createConnection();
        }

        private void getConfiguration()
        {
            orderPaymentUpdatedMessageQueue = configuration["RabbitMQ:OrderPaymentUpdatedQueue"];
            hostName = configuration["RabbitMQ:Host"];
            port = int.Parse(configuration["RabbitMQ:Port"]);
        }

        private void createConnection()
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                var factory = new ConnectionFactory()
                {
                    ClientProvidedName = "Ordering Payment Update Consumer",
                    HostName = hostName,
                    Port = port,
                };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
            }
        }

        public void RegisterConsumerAsync()
        {
            channel.QueueDeclare(queue: orderPaymentUpdatedMessageQueue, true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += PaymentUpdateMessageReceivedAsync;

            channel.BasicConsume(queue: orderPaymentUpdatedMessageQueue, autoAck: false, consumer: consumer);
        }

        private async void PaymentUpdateMessageReceivedAsync(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());

            ConsoleHelper.Writeline(message, ConsoleHelper.MessageType.Received);

            var result = await ProcessAsync(message);

            if (true)
            {
                channel.BasicAck(e.DeliveryTag, false);
            }
        }

        private async Task<bool> ProcessAsync(string body)
        {
            OrderPaymentUpdateMessage orderPaymentUpdateMessage =
                JsonConvert.DeserializeObject<OrderPaymentUpdateMessage>(body);

            await orderRepository.UpdateOrderPaymentStatus(orderPaymentUpdateMessage.OrderId, orderPaymentUpdateMessage.PaymentSuccess);

            return true;
        }

        public void Stop()
        {
            connection.Close();
        }

        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
