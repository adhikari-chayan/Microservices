using GloboTicket.Integration.Helpers;
using GloboTicket.Integration.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.Integration.MessagingBus
{
    public class RabbitMQMessageBus : IMessageBus
    {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _model;

        private string _queueName;
        private string _hostName;
        private string _userName;
        private string _password;
        private int _port;

        public RabbitMQMessageBus(string hostName, int port, string userName, string password)
        {
            _hostName = hostName;
            _userName = userName;
            _password = password;
            _port = port;
        }

        public Task PublishMessage(IntegrationBaseMessage message, string queueName)
        {
            _queueName = queueName;
            CreateConnection();
            SendMessage(message);

            return Task.CompletedTask;
        }

        private void CreateConnection()
        {
            _factory = new ConnectionFactory { ClientProvidedName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name, HostName = _hostName, Port = _port, UserName = _userName, Password = _password };
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.QueueDeclare(_queueName, true, false, false, null);
        }

        private void SendMessage(IntegrationBaseMessage message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            ConsoleHelper.Writeline(jsonMessage, ConsoleHelper.MessageType.Published);
            _model.BasicPublish("", _queueName, null, Encoding.UTF8.GetBytes(jsonMessage));
        }
    }
}