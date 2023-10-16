namespace AuthenticationService.RabbitMQServices
{
    public interface IMessageProducer
    {
        void SendMessage<T>(string queueName, T message);
    }
}
