
using ChatService.Repositories;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace ChatService.RabbitMQServices
{
    public class RabbitmqConsumer: DefaultBasicConsumer
    {
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly ServiceRepository _repository;
        public RabbitmqConsumer(IModel channel)
        {
            _channel = channel;
            _repository = new ServiceRepository();
        }

        public async override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            Console.WriteLine($"Consuming Message");
            Console.WriteLine(string.Concat("Message received from the exchange ", exchange));
            Console.WriteLine(string.Concat("Consumer tag: ", consumerTag));
            Console.WriteLine(string.Concat("Delivery tag: ", deliveryTag));
            Console.WriteLine(string.Concat("Routing tag: ", routingKey));
            Console.WriteLine(string.Concat("Message: ", JObject.Parse(Encoding.UTF8.GetString(body.Span))));
            JObject message = JObject.Parse(Encoding.UTF8.GetString(body.Span));
            _channel.BasicAck(deliveryTag, false);
            string request = (string)message["Message"];
            if (request == "SaveChat")
            {
                JObject data = (JObject)message["Data"];
                string tripId = data["TripId"].ToString();
                await _repository.Chat.StoreChat(tripId);
                Console.WriteLine("Store chat successfully");
            }
        }
    }

    
}
