namespace TripService.RabbitMQServices
{
    public interface IMessageProducer
    {
        void SendMessage<T>(string queueName, T message);
    }
}
