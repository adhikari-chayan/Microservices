using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Ordering.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GloboTicket.Services.Ordering.Messaging
{
    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private CheckoutUpdateConsumer checkoutConsumer;
        private PaymentUpdateConsumer paymentConsumer;

        public RabbitMQConsumer(IConfiguration configuration, IMessageBus messageBus, OrderRepository orderRepository)
        {
            checkoutConsumer = new CheckoutUpdateConsumer(configuration, messageBus, orderRepository);
            paymentConsumer = new PaymentUpdateConsumer(configuration, messageBus, orderRepository);
        }

        public void Start()
        {
            checkoutConsumer.RegisterConsumer();
            paymentConsumer.RegisterConsumerAsync();
        }

        public void Stop()
        {
            checkoutConsumer.Stop();
            paymentConsumer.Stop();
        }
    }
}
