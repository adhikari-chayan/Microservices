using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Payment.Messages;
using GloboTicket.Services.Payment.Model;
using GloboTicket.Services.Payment.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;


namespace GloboTicket.Services.Payment.Worker
{
    public class PaymentMessageWorker : RabMQServiceBase
    {
        private readonly ILogger<PaymentMessageWorker> logger;
        private readonly IExternalGatewayPaymentService externalGatewayPaymentService;
        private readonly IMessageBus messageBus;
        private string publishToQueueName;

        public PaymentMessageWorker(ILogger<PaymentMessageWorker> logger, 
            IConfiguration configuration, IExternalGatewayPaymentService externalGatewayPaymentService, IMessageBus messageBus) : base(logger, configuration)
        {
            try
            {
                this.externalGatewayPaymentService = externalGatewayPaymentService;
                this.messageBus = messageBus;

                base.QueueName = configuration["RabbitMQ:OrderPaymentRequestMessageQueue"];
                publishToQueueName = configuration["RabbitMQ:OrderPaymentUpdatedMessageQueue"];

                this.logger = logger;

            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
            }
        }

        public override async Task<bool> ProcessAsync(string messageBody)
        {
            OrderPaymentRequestMessage orderPaymentRequestMessage = JsonConvert.DeserializeObject<OrderPaymentRequestMessage>(messageBody);

            PaymentInfo paymentInfo = new PaymentInfo
            {
                CardNumber = orderPaymentRequestMessage.CardNumber,
                CardName = orderPaymentRequestMessage.CardName,
                CardExpiration = orderPaymentRequestMessage.CardExpiration,
                Total = orderPaymentRequestMessage.Total
            };

            var result = await externalGatewayPaymentService.PerformPayment(paymentInfo);
            
            OrderPaymentUpdateMessage orderPaymentUpdateMessage = new OrderPaymentUpdateMessage
            {
                PaymentSuccess = result,
                OrderId = orderPaymentRequestMessage.OrderId
            };

            try
            {
                await messageBus.PublishMessage(orderPaymentUpdateMessage, publishToQueueName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            logger.LogDebug($"{orderPaymentRequestMessage.OrderId}: ServiceBusListener received item.");
            logger.LogDebug($"{orderPaymentRequestMessage.OrderId}:  ServiceBusListener processed item.");

            return result;
        }
    }
}
