using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace InfoService.RabbitMQServices
{
    public class RabbitmqProducer : IMessageProducer
    {
        private readonly IConfiguration _configuration;

        public RabbitmqProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendMessage<T>(string queueName, T message)
        {
            //var factory = new ConnectionFactory { Uri = new Uri("amqps://gtyepqer:MFoGZBk-zqtRAf8fZoKPYIdBIcQTOp8T@fly.rmq.cloudamqp.com/gtyepqer") };
            //var connection = factory.CreateConnection();
            //using var channel = connection.CreateModel();
            //channel.QueueDeclare(queue: queueName, exclusive: false);
            //var json = JsonConvert.SerializeObject(message);
            //var body = Encoding.UTF8.GetBytes(json);
            //channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
    }
}
