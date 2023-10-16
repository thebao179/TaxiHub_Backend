using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using TripService.Repositories;

namespace TripService.RabbitMQServices
{
    public class RabbitmqConsumer : DefaultBasicConsumer
    {
        private readonly IModel _channel;
        private readonly ServiceRepository _serviceRepository;

        private readonly IConfiguration _configuration;
        public RabbitmqConsumer(IModel channel)
        {
            _channel = channel;
            _serviceRepository = new ServiceRepository();
         
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            Console.WriteLine($"Consuming Message");
            Console.WriteLine(string.Concat("Message received from the exchange ", exchange));
            Console.WriteLine(string.Concat("Consumer tag: ", consumerTag));
            Console.WriteLine(string.Concat("Delivery tag: ", deliveryTag));
            Console.WriteLine(string.Concat("Routing tag: ", routingKey));
            Console.WriteLine(string.Concat("Message: ", JObject.Parse(Encoding.UTF8.GetString(body.Span))));
            JObject message = JObject.Parse(Encoding.UTF8.GetString(body.Span));
            _channel.BasicAck(deliveryTag, false);
            Console.WriteLine(message);
        }
    }


}
