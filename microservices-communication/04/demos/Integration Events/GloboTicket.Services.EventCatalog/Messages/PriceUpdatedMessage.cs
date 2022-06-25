using System;
using GloboTicket.Integration.Messages;

namespace GloboTicket.Services.EventCatalog.Messages
{
    public class PriceUpdatedMessage: IntegrationBaseMessage
    {
        public Guid EventId { get; set; }
        public int Price { get; set; }
    }
}
