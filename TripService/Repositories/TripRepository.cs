using Helper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Globalization;
using TripService.FireBaseServices;
using TripService.Models;
using TripService.RabbitMQServices;
using static Helper.Catalouge;

namespace TripService.Repositories
{
    public class TripRepository : BaseRepository
    {
        private readonly IConfiguration _configuration;
        private readonly FirebaseService _fireBaseServices;
        private readonly RabbitmqProducer _rabbitmqProducer;
        //private readonly RabbitmqProducer _messageProducer;
        public TripRepository(TripDbContext context) : base(context)
        {
            _fireBaseServices = new FirebaseService();
            _rabbitmqProducer = new RabbitmqProducer();
            //_messageProducer = new RabbitmqProducer(_configuration);
        }

        public async Task<Guid> AcceptTrip(string driverId, string requestId)
        {
            TripRequest tripRequest = await context.TripRequest.FindAsync(Guid.Parse(requestId));
            if(tripRequest == null)
            {
                return Guid.Empty;
            }
            if(tripRequest.RequestStatus == Catalouge.Request.Canceled)
            {
                return Guid.Empty;
            }
            if(tripRequest.RequestStatus == Catalouge.Request.MovedToTrip)
            {
                return Guid.Empty;
            }
            tripRequest.RequestStatus = Catalouge.Request.MovedToTrip;
            Models.Trip trip = new()
            {
                TripId = Guid.NewGuid(),
                DriverId = Guid.Parse(driverId),
                PassengerId = tripRequest.PassengerId,
                StaffId = tripRequest.StaffId,
                VehicleId = Guid.NewGuid(),
                PassengerPhone = tripRequest.PassengerPhone,
                CreatedTime = DateTime.UtcNow,
                Destination = tripRequest.Destination,
                LatDesAddr = tripRequest.LatDesAddr,
                LongDesAddr = tripRequest.LongDesAddr,
                StartAddress = tripRequest.StartAddress,
                LatStartAddr = tripRequest.LatStartAddr,
                LongStartAddr = tripRequest.LongStartAddr,
                TripStatus = Catalouge.Trip.PickingUpCus,
                Distance = tripRequest.Distance,
                Price = tripRequest.Price,
                VehicleType = tripRequest.VehicleType,
                RequestId = tripRequest.RequestId,
            };
            await context.Trip.AddAsync(trip);
            _fireBaseServices.AddNewTrip(trip);
            _fireBaseServices.RemoveRequest(Guid.Parse(requestId));
            await context.SaveChangesAsync();
            return trip.TripId;
        }

        public async Task<int> PickedPassenger(Guid tripId)
        {
            Models.Trip trip = await context.Trip.FindAsync(tripId);
            if (trip != null)
            {
                if (trip.TripStatus != Catalouge.Trip.PickingUpCus)
                {
                    return 0;
                }
                trip.TripStatus = Catalouge.Trip.OnTheWay;
                _fireBaseServices.UpdateOnGoingTrip(trip);
                return await context.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<Models.Trip> GetTripForPassenger(Guid passengerId, Guid requestId)
        {
            Models.Trip trip = await context.Trip.FirstOrDefaultAsync(t => t.PassengerId == passengerId && t.RequestId == requestId);
            return trip;
        }

        public async Task<List<Models.Trip>> GetListTripsByDriver(Guid driverId)
        {
            List<Models.Trip> trips = await context.Trip.Where(t => t.DriverId == driverId).ToListAsync(); 
            return trips;
        }

        public async Task<List<Models.Trip>> GetListTripsByPassenger(Guid passengerId)
        {
            List<Models.Trip> trips = await context.Trip.Where(t => t.PassengerId == passengerId).ToListAsync();
            return trips;
        }

        public async Task<int> CancelTrip(Guid tripId)
        {
            Models.Trip trip = await context.Trip.FindAsync(tripId);
            if(trip == null)
            {
                return 0;
            }
            trip.TripStatus = Catalouge.Trip.CanceledByDriver;
            _fireBaseServices.RemoveTrip(tripId);
            _rabbitmqProducer.SendMessage("chat", new
            {
                Status = true,
                Message = "SaveChat",
                Data = new
                {
                    TripId = tripId.ToString(),
                },
            });
            return await context.SaveChangesAsync();
        }

        public async Task<Models.Trip> GetTrip(Guid tripId)
        {
            Models.Trip trip = await context.Trip.FindAsync(tripId);
            return trip;
        }

        public async Task<object> GetCompletedTrips(Guid driverId, string from, string to)
        {
            DateTime fromTime = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
            DateTime toTime = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
            if (fromTime == toTime)
            {
                DateTime startDateTime = fromTime; //Today at 00:00:00
                DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
                List<Models.Trip> driverTodayTrips = context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToList();
                return new
                {
                    total = driverTodayTrips.Count,
                    trips = driverTodayTrips
                };
            }
            toTime = toTime.AddDays(1).AddTicks(-1);
            List<Models.Trip> driverTrips = context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime && t.TripStatus == Catalouge.Trip.Done).ToList();

            return new { 
                total = driverTrips.Count,
                trips = driverTrips
            };
        }

        public async Task<int> GetIncome(Guid driverId, string from, string to)
        {
            int totalPrice = 0;
            DateTime fromTime = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
            DateTime toTime = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
            if(fromTime == toTime)
            {
                DateTime startDateTime = fromTime; //Today at 00:00:00
                DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
                List<Models.Trip> driverTodayTrips = context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToList();
                foreach (var trip in driverTodayTrips)
                {
                    if (trip.Price != null)
                    {
                        totalPrice += trip.Price.Value;
                    }
                }
                return totalPrice;
            }
            toTime = toTime.AddDays(1).AddTicks(-1);
            List<Models.Trip> driverTrips = context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime &&  t.TripStatus == Catalouge.Trip.Done).ToList();
            foreach(var trip in driverTrips)
            {
                if (trip.Price != null)
                {
                    totalPrice += trip.Price.Value;
                }
            }

            return totalPrice;
        }

        public async Task<int> CompleteTrip(Guid tripId)
        {
            Models.Trip trip = await context.Trip.FindAsync(tripId);
            if(trip == null || trip.TripStatus == Catalouge.Trip.Done)
            {
                return 0;
            }
            trip.TripStatus = Catalouge.Trip.Done;
            trip.CompleteTime = DateTime.Now;
            _fireBaseServices.RemoveTrip(tripId);
            _rabbitmqProducer.SendMessage("chat", new
            {
                Status = true,
                Message = "SaveChat",
                Data = new
                {
                    TripId = tripId.ToString(),
                },
            });
            return await context.SaveChangesAsync();
        }

        public async Task<List<Models.Trip>> GetTrips()
        {
            return await context.Trip.ToListAsync();
        }



        public async Task<int> CalcNumOfPagesForPassenger(Guid passengerId, int pageSize)
        {
            int totalRecords = await context.Trip.Where(t => t.PassengerId == passengerId).CountAsync();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<int> CalcNumOfPagesForDriver(Guid driverId, int pageSize)
        {
            int totalRecords = await context.Trip.Where(t => t.DriverId == driverId).CountAsync();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<List<Models.Trip>> GetPassengerTripsPaging(Guid passengerId, int pageSize, int pageNum)
        {
            var passengerTrips = context.Trip
                            .Where(p => p.PassengerId == passengerId)
                            .OrderBy(p => p.CreatedTime)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return passengerTrips;
        }

        public async Task<List<Models.Trip>> GetDriverTripsPaging(Guid driverId, int pageSize, int pageNum)
        {
            var driverTrips = context.Trip
                            .Where(p => p.DriverId == driverId)
                            .OrderBy(p => p.CreatedTime)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return driverTrips;
        }


        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.Trip);
            return await context.SaveChangesAsync();
        }
    }
}
