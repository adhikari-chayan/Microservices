using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Payment.Messages;
using GloboTicket.Services.Payment.Model;
using GloboTicket.Services.Payment.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GloboTicket.Services.Payment.Worker
{
    public class old : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        //private ISubscriptionClient subscriptionClient;
        private readonly IExternalGatewayPaymentService externalGatewayPaymentService;
        private readonly IMessageBus messageBus;
        private readonly string orderPaymentUpdatedMessageTopic;

        private string _hostName;
        private string _userName;
        private string _password;

        private string _listeningQueueName;
        private string _publishToQueueName;

        private int _port;
        private IConnection _connection;
        private IModel _channel;

        public old(IConfiguration configuration, ILoggerFactory loggerFactory, IExternalGatewayPaymentService externalGatewayPaymentService, IMessageBus messageBus)
        {
            logger = loggerFactory.CreateLogger<old>();
            _publishToQueueName = configuration["RabbitMQ:OrderPaymentUpdatedMessageQueue"];

            this.configuration = configuration;
            this.externalGatewayPaymentService = externalGatewayPaymentService;
            this.messageBus = messageBus;

            _hostName = configuration["RabbitMQ:Host"];
            _port = int.Parse(configuration["RabbitMQ:Port"]);
            _listeningQueueName = configuration["RabbitMQ:OrderPaymentRequestMessageQueue"];
            _userName = configuration["RabbitMQ:Username"];
            _password = configuration["RabbitMQ:Password"];

            CreateConnection();

        }

        private void CreateConnection()
        {
            if (!string.IsNullOrEmpty(_hostName))
            {
                var factory = new ConnectionFactory()
                {
                    ClientProvidedName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    HostName = _hostName,
                    Port = _port,
                    UserName = _userName,
                    Password = _password
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(_listeningQueueName, true, false, false, null);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && !string.IsNullOrEmpty(_hostName))
            {
                Register();
            }

            return Task.CompletedTask;
        }

        public void Register()
        {

            //_channel.BasicQos(0, 1, false)
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ProcessMessageAsync;
            _channel.BasicConsume(_listeningQueueName, false, consumer);
        }

        protected async void ProcessMessageAsync(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var messageBody = Encoding.UTF8.GetString(body.ToArray());

            OrderPaymentRequestMessage orderPaymentRequestMessage = JsonConvert.DeserializeObject<OrderPaymentRequestMessage>(messageBody);

            PaymentInfo paymentInfo = new PaymentInfo
            {
                CardNumber = orderPaymentRequestMessage.CardNumber,
                CardName = orderPaymentRequestMessage.CardName,
                CardExpiration = orderPaymentRequestMessage.CardExpiration,
                Total = orderPaymentRequestMessage.Total
            };

            var result = await externalGatewayPaymentService.PerformPayment(paymentInfo);

            if (result)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }

            //send payment result to order service via service bus
            OrderPaymentUpdateMessage orderPaymentUpdateMessage = new OrderPaymentUpdateMessage
            {
                PaymentSuccess = result,
                OrderId = orderPaymentRequestMessage.OrderId
            };

            try
            {
                await messageBus.PublishMessage(orderPaymentUpdateMessage, _publishToQueueName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            logger.LogDebug($"{orderPaymentRequestMessage.OrderId}: ServiceBusListener received item.");
            await Task.Delay(20000);
            logger.LogDebug($"{orderPaymentRequestMessage.OrderId}:  ServiceBusListener processed item.");
        }
    }
}
