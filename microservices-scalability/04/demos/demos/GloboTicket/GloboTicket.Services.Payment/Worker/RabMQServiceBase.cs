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
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabMQServiceBase> _logger;
        private readonly string _hostName;
        private readonly int _port;

        //protected string RouteKey;
        protected string QueueName;

        public RabMQServiceBase(ILogger<RabMQServiceBase> logger, IConfiguration configuration)
        {
            try
            {
                _logger = logger;
                _hostName = configuration["RabbitMQ:Host"];
                _port = int.Parse(configuration["RabbitMQ:Port"]);

                if (!string.IsNullOrEmpty(_hostName))
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = _hostName,
                        Port = _port,
                    };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RabbitListener init error,ex:{ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && !string.IsNullOrEmpty(_hostName))
            {
                Register();

                await Task.Delay(1000, stoppingToken);
            }
        }

        public void Register()
        {
            //_channel.ExchangeDeclare(exchange: "AccountManager", type: "direct");
            _channel.QueueDeclare(queue: QueueName, true, false, false, null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += MessageReceived;

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }

        private async void MessageReceived(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());

            ConsoleHelper.Writeline(message, ConsoleHelper.MessageType.Received);
            var result = await ProcessAsync(message);

            if (result)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        public virtual async Task<bool> ProcessAsync(string message)
        {
            throw new NotImplementedException();
        }

        public void DeRegister()
        {
            _connection.Close();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _connection.Close();
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation($"Worker disposed at: {DateTime.Now}");
            _channel.Dispose();
            _connection.Dispose();
            base.Dispose();
        }
    }
}