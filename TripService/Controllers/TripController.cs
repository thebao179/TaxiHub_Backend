using Helper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TripService.DTOs;
using TripService.Models;
using static Helper.Catalouge;
using static Helper.Redis;
using Docker.DotNet;
using Helper;
using Newtonsoft.Json;
using TripService.DataAccess;
using System.Security.Claims;

namespace TripService.Controllers
{
    [Route("api/trip/[controller]/[action]")]
    [ApiController]
    public class TripController : BaseController
    {
        private readonly TripDataAccess _dataAccess;
        private readonly TripRequestDataAccess _requestDataAccess;
        private readonly TripFeedbackDataAccess _feedbackDataAccess;
        public TripController()
        {
            _dataAccess = new TripDataAccess(); 
            _requestDataAccess = new TripRequestDataAccess();
            _feedbackDataAccess = new TripFeedbackDataAccess();
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<ResponseMsg> AcceptRequest(AcceptTripDTO acceptTripDTO)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Guid tripId = await _dataAccess.AcceptTrip(UserId.ToString(), acceptTripDTO.DriverId, acceptTripDTO.RequestId);
            //Guid tripId = await Repository.Trip.AcceptTrip(acceptTripDTO.DriverId, acceptTripDTO.RequestId);
            return new ResponseMsg
            {
                status = tripId != Guid.Empty ? true : false,
                data = tripId,
                message = tripId != Guid.Empty ? "Accept request successfully" : "Failed to accept this request",
            };
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<ResponseMsg> FinishTrip([FromBody] object tripIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripIdJson.ToString());
            string tripId = (string)objTemp["tripId"];
            //int result = await Repository.Trip.CompleteTrip(Guid.Parse(tripId));
            int result = await _dataAccess.CompleteTrip(UserId.ToString(), Guid.Parse(tripId));
            return new ResponseMsg
            {
                status = result > 0 ? true : false,
                data = null,
                message = result > 0 ? "Trip is finished" : "Failed to finish trip",
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<ResponseMsg> CancelTrip([FromBody] object tripIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripIdJson.ToString());
            string tripId = (string)objTemp["tripId"];
            //int result = await Repository.Trip.CancelTrip(Guid.Parse(tripId));
            int result = await _dataAccess.CancelTrip(UserId.ToString(), Guid.Parse(tripId));
            return new ResponseMsg {
                status = result > 0 ? true : false,
                data = null,
                message = result > 0 ? "Cancel trip successfully" : "Failed to cancel this trip"
            };
            
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetPassengersTripTotalPages([FromBody] object tripJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripJson.ToString());
            string passengerId = (string)objTemp["passengerId"];
            int pageSize = Convert.ToInt32(objTemp["pageSize"]);
            int pageNum = await _dataAccess.CalcNumOfPagesForPassenger(UserId.ToString(), Guid.Parse(passengerId), pageSize);
            //int pageNum = await Repository.Trip.CalcNumOfPagesForPassenger(Guid.Parse(passengerId), pageSize);
            return new ResponseMsg
            {
                status = true,
                data = pageNum,
                message = "Get num of pages successfully"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetDriverTripTotalPages([FromBody] object tripJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripJson.ToString());
            string driverId = (string)objTemp["driverId"];
            int pageSize = Convert.ToInt32(objTemp["pageSize"]);
            int pageNum = await _dataAccess.CalcNumOfPagesForDriver(UserId.ToString(), Guid.Parse(driverId), pageSize);
            return new ResponseMsg
            {
                status = true,
                data = pageNum,
                message = "Get num of pages successfully"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetPassengersTripPaging([FromBody] object tripJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripJson.ToString());
            string passengerId = (string)objTemp["passengerId"];
            int pageSize = Convert.ToInt32(objTemp["pageSize"]);
            int pageNum = Convert.ToInt32(objTemp["pageNum"]);
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            if(pageSize == 0)
            {
                pageSize = 15;
            }
            int totalPage = await _dataAccess.CalcNumOfPagesForPassenger(UserId.ToString(), Guid.Parse(passengerId), pageSize);
            //int totalPage = await Repository.Trip.CalcNumOfPagesForPassenger(Guid.Parse(passengerId), pageSize);
            if (pageNum > totalPage)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = $"The pageNum you input is greater than the max number of pages, try another pageSize or smaller pageNum"
                };
            }
            return new ResponseMsg
            {
                status = true,
                data = await _dataAccess.GetPassengerTripsPaging(UserId.ToString(), Guid.Parse(passengerId), pageSize, pageNum),
                message = "Get num of pages successfully"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetDriverTripPageing([FromBody] object tripJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripJson.ToString());
            string driverId = (string)objTemp["driverId"];
            int pageSize = Convert.ToInt32(objTemp["pageSize"]);
            int pageNum = Convert.ToInt32(objTemp["pageNum"]);
            if (pageNum == 0)
            {
                pageNum = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 15;
            }
            int totalPage = await _dataAccess.CalcNumOfPagesForDriver(UserId.ToString(), Guid.Parse(driverId), pageSize);
            //int totalPage = await Repository.Trip.CalcNumOfPagesForDriver(Guid.Parse(driverId), pageSize);
            if (pageNum > totalPage)
            {
                return new ResponseMsg
                {
                    status = false,
                    data = null,
                    message = $"The pageNum you input is greater than the max number of pages, try another pageSize or smaller pageNum"
                };
            }
            return new ResponseMsg
            {
                status = true,
                data = await _dataAccess.GetDriverTripsPaging(UserId.ToString(), Guid.Parse(driverId), pageSize, pageNum),
                message = "Get num of pages successfully"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetPassengerTrips([FromBody] object passengerIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(passengerIdJson.ToString());
            string passengerId = (string)objTemp["passengerId"];
            List<Models.Trip> trips = await _dataAccess.GetListTripsByPassenger(UserId.ToString(), Guid.Parse(passengerId));
            //List<Models.Trip> trips = await Repository.Trip.GetListTripsByPassenger(Guid.Parse(passengerId));
            return new ResponseMsg
            {
                status = trips != null ? true : false,
                data = trips,
                message = trips != null ? "Get trips by passenger successfully" : "Failed to get trips by passenger"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetDriverTrips([FromBody] object driverIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(driverIdJson.ToString());
            string driverId = (string)objTemp["driverId"];
            List<Models.Trip> trips = await _dataAccess.GetListTripsByDriver(UserId.ToString(), Guid.Parse(driverId));
            //List<Models.Trip> trips = await Repository.Trip.GetListTripsByDriver(Guid.Parse(driverId));
            return new ResponseMsg
            {
                status = trips != null ? true : false,
                data = trips,
                message = trips != null ? "Get trips by driver successfully" : "Failed to get trips by driver"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetCurrentTrip([FromBody]object tripIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripIdJson.ToString());
            string tripId = (string)objTemp["tripId"];
            Models.Trip result = await _dataAccess.GetTrip(UserId.ToString(), Guid.Parse(tripId));
            //Models.Trip result = await Repository.Trip.GetTrip(Guid.Parse(tripId));
            return new ResponseMsg
            {
                status = result != null ? true : false,
                data = result,
                message = result != null ? "Get trip successfully" : "Failed to get trip",
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> GetCurrentTripWithCaching([FromBody] object tripIdJson)
        {
            JObject objTemp = JObject.Parse(tripIdJson.ToString());
            string tripId = (string)objTemp["tripId"];
            string val = Redis.GetRedisVal(tripId);
            if (val != "")
            {
                Models.Trip deserializedTrip = JsonConvert.DeserializeObject<Models.Trip>(val);
                return new ResponseMsg
                {
                    status = true,
                    data = deserializedTrip,
                    message = "Get trip successfully, this trip already stayed in cache memmory"
                };
            }
            else
            {
                Models.Trip result = await Repository.Trip.GetTrip(Guid.Parse(tripId));
                if(result != null)
                {
                    string json = JsonConvert.SerializeObject(result);
                    Redis.SetRedisVal(tripId, json, 5);
                    return new ResponseMsg
                    {
                        status = true,
                        data = result,
                        message = "Get trip successfully, this trip is just added to cache memory",
                    };
                }
                else
                {
                    return new ResponseMsg
                    {
                        status = false,
                        data = null,
                        message = "Failed to get trip, this trip doesn't exist so it won't be added into cache memory",
                    };
                }
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetCurrentTripForPassenger([FromBody]object passengerTrip)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(passengerTrip.ToString());
            string passengerId = (string)objTemp["passengerId"];
            string requestId = (string)objTemp["requestId"];
            Models.Trip trip = await _dataAccess.GetTripForPassenger(UserId.ToString(), Guid.Parse(passengerId), Guid.Parse(requestId));
            //Models.Trip trip = await Repository.Trip.GetTripForPassenger(Guid.Parse(passengerId), Guid.Parse(requestId));
            return new ResponseMsg
            {
                status = trip != null ? true : false,
                data = trip,
                message = trip != null ? "Get trip successfully" : "Failed to get trip",
            };
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<ResponseMsg> PickedPassenger([FromBody] object tripIdJson)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(tripIdJson.ToString());
            string tripId = (string)objTemp["tripId"];
            int result = await _dataAccess.PickedPassenger(UserId.ToString(), Guid.Parse(tripId));
            //int result = await Repository.Trip.PickedPassenger(Guid.Parse(tripId));
            return new ResponseMsg
            {
                status = result > 0 ? true : false,
                data = result,
                message = result > 0 ? "Update status picked passenger successfully" : "Failed to update status picked passenger"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetTrips()
        {
            return new ResponseMsg
            {
                status = true,
                data = await _dataAccess.GetTrips(),
                message = "Get trips successfully"
            };
        }


        [HttpGet]
        [Authorize(Roles = "Driver")]
        public async Task<ResponseMsg> GetIncome([FromBody]object getIncome)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(getIncome.ToString());
            string driverId = (string)objTemp["driverId"];
            string from = (string)objTemp["from"];
            string to = (string)objTemp["to"];
            int income = await _dataAccess.GetIncome(UserId.ToString(), Guid.Parse(driverId), from, to);
            //int income = await Repository.Trip.GetIncome(Guid.Parse(driverId), from, to);
            return new ResponseMsg
            {
                status = true,
                data = income,
                message = "Get income successfully"
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> TestCaching(string id)
        {
            string val = Redis.GetRedisVal(id);
            if (val != "")
            {
                return new ResponseMsg
                {
                    status = true,
                    data = new
                    {
                        value = val
                    },
                    message = "Test caching success, this value is already stored in cache memory"
                };
            }

            string response = "This is a caching value";
            Redis.SetRedisVal(id, response, 10);
            return new ResponseMsg
            {
                status = true,
                data = new
                {
                    value = response
                },
                message = "Save value to redis success"
            };
        }

        [HttpGet]
        public async Task<ResponseMsg> TestLoadBalancing()
        {
            // create a DockerClient instance
            var dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

            // get the container ID for the current running container
            var containerID = Environment.GetEnvironmentVariable("HOSTNAME");

            // get the container port information
            //var containerResponse = await dockerClient.Containers.InspectContainerAsync(containerID);

            return new ResponseMsg
            {
                status = true,
                data = new { containerID = containerID },
                message = $"The request is handled on the container which have the containerID = {containerID}"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> GetCompletedTrips([FromBody]object getCompletedTrips)
        {
            Guid UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            JObject objTemp = JObject.Parse(getCompletedTrips.ToString());
            string driverId = (string)objTemp["driverId"];
            string from = (string)objTemp["from"];
            string to = (string)objTemp["to"];
            var result = await _dataAccess.GetCompletedTrips(UserId.ToString(), Guid.Parse(driverId), from, to);
            //var result = await Repository.Trip.GetCompletedTrips(Guid.Parse(driverId), from, to);
            if (result != null)
            {
                return new ResponseMsg
                {
                    status = true,
                    data = result,
                    message = "Get completed trips successfully"
                };
            }
            return new ResponseMsg
            {
                status = false,
                data = new
                {
                    total = 0,
                    trips = new List<string> { }
                },
                message = "Failed to get completed trips"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMsg> ClearDb()
        {
            await _dataAccess.ClearTable();
            await _requestDataAccess.ClearTable();
            await _feedbackDataAccess.ClearTable();

            return new ResponseMsg
            {
                status = true,
                data = null,
                message = "Executed clear all database of Trip service"
            };
        }
    }
}
