using GloboTicket.Integration.Helpers;
using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Ordering.Entities;
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
    public class CheckoutUpdateConsumer : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly OrderRepository orderRepository;
        private readonly IMessageBus messageBus;

        private string checkoutMessageQueue;
        private string orderPaymentRequestMessageQueue;
        private string hostName;
        private int port;

        private IConnection connection;
        private IModel channel;
        private ILogger<CheckoutUpdateConsumer> logger;

        public CheckoutUpdateConsumer(IConfiguration configuration, IMessageBus messageBus, OrderRepository orderRepository)
        {
            this.configuration = configuration;
            this.orderRepository = orderRepository;
            this.messageBus = messageBus;

            getConfiguration();
            createConnection();
        }

        private void getConfiguration()
        {
            checkoutMessageQueue = configuration["RabbitMQ:CheckoutQueue"];
            orderPaymentRequestMessageQueue = configuration["RabbitMQ:OrderPaymentRequestQueue"];
            hostName = configuration["RabbitMQ:Host"];
            port = int.Parse(configuration["RabbitMQ:Port"]);
        }

        private void createConnection()
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                var factory = new ConnectionFactory()
                {
                    ClientProvidedName = "Ordering Checkout Consumer",
                    HostName = hostName,
                    Port = port,
                };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
            }
        }

        public void RegisterConsumer() 
        {
            channel.QueueDeclare(queue: checkoutMessageQueue, true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += CheckOutMessageReceived;

            channel.BasicConsume(queue: checkoutMessageQueue, autoAck: false, consumer: consumer);
        }

        private async void CheckOutMessageReceived(object sender, BasicDeliverEventArgs e)
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
            //save order with status not paid
            BasketCheckoutMessage basketCheckoutMessage = JsonConvert.DeserializeObject<BasketCheckoutMessage>(body);

            Guid orderId = Guid.NewGuid();

            Order order = new Order
            {
                UserId = basketCheckoutMessage.UserId,
                Id = orderId,
                OrderPaid = false,
                OrderPlaced = DateTime.Now,
                OrderTotal = basketCheckoutMessage.BasketTotal
            };

            await orderRepository.AddOrder(order);

            //send order payment request message
            OrderPaymentRequestMessage orderPaymentRequestMessage = new OrderPaymentRequestMessage
            {
                CardExpiration = basketCheckoutMessage.CardExpiration,
                CardName = basketCheckoutMessage.CardName,
                CardNumber = basketCheckoutMessage.CardNumber,
                OrderId = orderId,
                Total = basketCheckoutMessage.BasketTotal
            };

            try
            {
                await messageBus.PublishMessage(orderPaymentRequestMessage, orderPaymentRequestMessageQueue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

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
