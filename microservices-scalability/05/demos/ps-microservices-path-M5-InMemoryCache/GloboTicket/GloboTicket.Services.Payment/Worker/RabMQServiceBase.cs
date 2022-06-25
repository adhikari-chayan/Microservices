using GloboTicket.Integration.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GloboTicket.Services.Payment.Worker
{
    public class RabMQServiceBase : BackgroundService
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly ILogger<RabMQServiceBase> logger;
        private readonly string hostName;
        private readonly int port;

        //protected string RouteKey;
        protected string QueueName;

        public RabMQServiceBase(ILogger<RabMQServiceBase> logger, IConfiguration configuration)
        {
            try
            {
                this.logger = logger;
                hostName = configuration["RabbitMQ:Host"];
                port = int.Parse(configuration["RabbitMQ:Port"]);

                if (!string.IsNullOrEmpty(hostName))
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = hostName,
                        Port = port,
                    };
                    connection = factory.CreateConnection();
                    channel = connection.CreateModel();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"RabbitListener init error,ex:{ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && !string.IsNullOrEmpty(hostName))
            {
                Register();

                await Task.Delay(1000, stoppingToken);
            }
        }

        public void Register()
        {
            channel.QueueDeclare(queue: QueueName, true, false, false, null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += MessageReceived;

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }

        private async void MessageReceived(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());

            ConsoleHelper.Writeline(message, ConsoleHelper.MessageType.Received);
            var result = await ProcessAsync(message);

            if (result)
            {
                channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        public virtual async Task<bool> ProcessAsync(string message)
        {
            throw new NotImplementedException();
        }

        public void DeRegister()
        {
            connection.Close();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            connection.Close();
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            logger.LogInformation($"Worker disposed at: {DateTime.Now}");
            channel.Dispose();
            connection.Dispose();
            base.Dispose();
        }
    }
}