using Helper;
using InfoService.Models;
using InfoService.Repositories;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace InfoService.RabbitMQServices
{
    public class RabbitmqConsumer : DefaultBasicConsumer
    {
        private readonly IModel _channel;
        private readonly ServiceRepository _serviceRepository;
        private readonly RabbitmqProducer _producer;
        private readonly IConfiguration _configuration;
        public RabbitmqConsumer(IModel channel)
        {
            _channel = channel;
            _serviceRepository = new ServiceRepository();
            _producer = new RabbitmqProducer(_configuration);
        }

        public override async void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            Console.WriteLine(string.Concat("Message: ", JObject.Parse(Encoding.UTF8.GetString(body.Span))));
            JObject message = JObject.Parse(Encoding.UTF8.GetString(body.Span));
            _channel.BasicAck(deliveryTag, false);
            string status = (string)message["Status"];
            string messageReceveid = (string)message["Message"];
            JObject data = (JObject)message["Data"];
            switch (messageReceveid) 
            {
                case "AddDataInfo":
                    string Role = (string)data["Role"];
                    if (Role == Catalouge.Role.Staff)
                    {
                        Staff newUser = new Staff
                        {
                            AccountId = (Guid)data["AccountId"],
                            Email = (string)data["Email"],
                            Phone = (string)data["Phone"],
                            Name = (string)data["Name"],
                        };
                        await _serviceRepository.Staff.AddStaffInfo(newUser);
                    }
                    else if (Role == Catalouge.Role.Passenger)
                    {
                        Passenger newUser = new Passenger
                        {
                            AccountId = (Guid)data["AccountId"],
                            Email = (string)data["Email"],
                            Phone = (string)data["Phone"],
                            Name = (string)data["Name"],
                        };
                        await _serviceRepository.Passenger.AddPassengerInfo(newUser);
                    }
                    else if (Role == Catalouge.Role.Driver)
                    {
                        Driver newUser = new Driver
                        {
                            AccountId = (Guid)data["AccountId"],
                            Email = (string)data["Email"],
                            Phone = (string)data["Phone"],
                            Name = (string)data["Name"],
                        };
                        await _serviceRepository.Driver.AddDriverInfo(newUser);
                    }
                    break;
                case "GetPassengerInfo":
                    break;
                case "GetDriverInfo":
                    break;
                case "GetStaffInfo":
                    break;
                case "GetVehicleInfo":
                    break;
                case "GetTripInfo":
                    Guid driverId = (Guid)data["DriverId"];
                    Guid passengerId = (Guid)data["PassengerId"];
                    Guid staffId = (Guid)data["StaffId"];
                    Guid vehicleId = (Guid)data["VehicleId"];
                    _producer.SendMessage("info", new
                    {
                        driver = await _serviceRepository.Driver.GetDriverById(driverId),
                        passenger = await _serviceRepository.Passenger.GetPassengerById(passengerId),
                        staff = await _serviceRepository.Staff.GetStaffById(staffId),
                        vehicle = await _serviceRepository.Vehicle.GetDriverVehicle(driverId)
                    }); ;
                    break;
                default:
                    break;
            }
            

        }
        }
}
