namespace GloboTicket.Services.Ordering.Messaging
{
    public interface IRabbitMQConsumer
    {
        void Start();
        void Stop();
    }
}