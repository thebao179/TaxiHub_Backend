using Helper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TripService.FireBaseServices;
using TripService.Models;
using TripService.RabbitMQServices;
using static Helper.Catalouge;

namespace TripService.DataAccess
{
    public class TripDataAccess
    {
        private readonly List<string> _connectionStrings = new List<string>();
        private readonly FirebaseService _fireBaseServices;
        private readonly RabbitmqProducer _rabbitmqProducer;


        public TripDataAccess()
        {
            _fireBaseServices = new FirebaseService();
            _rabbitmqProducer = new RabbitmqProducer();
            var connectionStrings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString1"));
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString2"));
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString3"));
        }

        private string GetConnectionString(string userId)
        {
            int userid = Guid.Parse(userId).GetHashCode();
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(userid.ToString()));
            var x = BitConverter.ToUInt16(hash, 0) % _connectionStrings.Count;
            return _connectionStrings[x];
        }

        public async Task<List<Models.Trip>> GetTrips()
        {
            List<Models.Trip> tripList = new List<Models.Trip>();
            foreach (var connectionString in _connectionStrings)
            {
                using var dbContext = new TripServiceContext(connectionString);
                List<Models.Trip> temp = await dbContext.Trip.ToListAsync();
                tripList.AddRange(temp);
            }
            return tripList;
        }

        public async Task<Guid> AcceptTrip(string userId, string driverId, string requestId)
        {
            string currentConnectionString = "";
            TripRequest request = new TripRequest();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                request = await context.TripRequest.FindAsync(Guid.Parse(requestId));
                if(request != null)
                {
                    currentConnectionString = connectionString;
                    break;
                }
            }
            using var dbContext = new TripServiceContext(currentConnectionString);
            TripRequest tripRequest = await dbContext.TripRequest.FindAsync(Guid.Parse(requestId));
            if (tripRequest == null)
            {
                return Guid.Empty;
            }
            if (tripRequest.RequestStatus == Catalouge.Request.Canceled)
            {
                return Guid.Empty;
            }
            if (tripRequest.RequestStatus == Catalouge.Request.MovedToTrip)
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
            await dbContext.Trip.AddAsync(trip);
            _fireBaseServices.AddNewTrip(trip);
            _fireBaseServices.RemoveRequest(Guid.Parse(requestId));
            await dbContext.SaveChangesAsync();
            return trip.TripId;
        }

        public async Task<int> PickedPassenger(string userId, Guid tripId)
        {
            string currentConnectionString = "";
            Models.Trip findTrip = new Models.Trip();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                findTrip = await context.Trip.FindAsync(tripId);
                if (findTrip != null)
                {
                    currentConnectionString = connectionString;
                    break;
                }
            }
            using var dbContext = new TripServiceContext(currentConnectionString);
            Models.Trip trip = await dbContext.Trip.FindAsync(tripId);
            if (trip != null)
            {
                if (trip.TripStatus != Catalouge.Trip.PickingUpCus)
                {
                    return 0;
                }
                trip.TripStatus = Catalouge.Trip.OnTheWay;
                _fireBaseServices.UpdateOnGoingTrip(trip);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<Models.Trip> GetTripForPassenger(string userId, Guid passengerId, Guid requestId)
        {
            Models.Trip trip = new Models.Trip();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                trip = await context.Trip.FirstOrDefaultAsync(t => t.PassengerId == passengerId && t.RequestId == requestId);
                if(trip != null)
                {
                    return trip;
                }
            }
            return new Models.Trip();
        }

        public async Task<List<Models.Trip>> GetListTripsByDriver(string userId, Guid driverId)
        {
            List<Models.Trip> tripList = new List<Models.Trip>();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> temp = await context.Trip.Where(t => t.DriverId == driverId).ToListAsync();
                tripList.AddRange(temp);
            }
            return tripList.OrderBy(t => t.CreatedTime).ToList();
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //List<Models.Trip> trips = await dbContext.Trip.Where(t => t.DriverId == driverId).ToListAsync();
            //return trips;
        }

        public async Task<List<Models.Trip>> GetListTripsByPassenger(string userId, Guid passengerId)
        {
            List<Models.Trip> tripList = new List<Models.Trip>();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> temp = await context.Trip.Where(t => t.PassengerId == passengerId).ToListAsync();
                tripList.AddRange(temp);
            }
            return tripList.OrderBy(t => t.CreatedTime).ToList();
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //List<Models.Trip> trips = await dbContext.Trip.Where(t => t.PassengerId == passengerId).ToListAsync();
            //return trips;
        }

        public async Task<int> CancelTrip(string userId, Guid tripId)
        {
            string currentConnectionString = "";
            Models.Trip findTrip = new Models.Trip();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                findTrip = await context.Trip.FindAsync(tripId);
                if (findTrip != null)
                {
                    currentConnectionString = connectionString;
                    break;
                }
            }
            using var dbContext = new TripServiceContext(currentConnectionString);
            Models.Trip trip = await dbContext.Trip.FindAsync(tripId);
            if (trip == null)
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
            return await dbContext.SaveChangesAsync();
        }

        public async Task<Models.Trip> GetTrip(string userId, Guid tripId)
        {
            string currentConnectionString = "";
            Models.Trip findTrip = new Models.Trip();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                findTrip = await context.Trip.FindAsync(tripId);
                if (findTrip != null)
                {
                    currentConnectionString = connectionString;
                    break;
                }
            }
            using var dbContext = new TripServiceContext(currentConnectionString);
            Models.Trip trip = await dbContext.Trip.FindAsync(tripId);
            return trip;
        }

        public async Task<object> GetCompletedTrips(string userId, Guid driverId, string from, string to)
        {
            List<Models.Trip> completedTrips = new List<Models.Trip>();
            DateTime fromTime = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
            DateTime toTime = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
            if(fromTime == toTime) 
            {
                DateTime startDateTime = fromTime; //Today at 00:00:00
                DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
                foreach (var connectionString in _connectionStrings)
                {
                    using var context = new TripServiceContext(connectionString);
                    List<Models.Trip> trips = await context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToListAsync();
                    completedTrips.AddRange(trips);
                }
                return new
                {
                    total = completedTrips.Count,
                    trips = completedTrips
                };
            }
            toTime = toTime.AddDays(1).AddTicks(-1);
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> trips = await context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime && t.TripStatus == Catalouge.Trip.Done).ToListAsync();
                completedTrips.AddRange(trips);
            }
            return new
            {
                total = completedTrips.Count,
                trips = completedTrips
            };


            //if (fromTime == toTime)
            //{
            //    using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //    DateTime startDateTime = fromTime; //Today at 00:00:00
            //    DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
            //    List<Models.Trip> driverTodayTrips = dbContext.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToList();
            //    return new
            //    {
            //        total = driverTodayTrips.Count,
            //        trips = driverTodayTrips
            //    };
            //}
            //toTime = toTime.AddDays(1).AddTicks(-1);
            //List<Models.Trip> driverTrips = dbContext.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime && t.TripStatus == Catalouge.Trip.Done).ToList();

            //return new
            //{
            //    total = driverTrips.Count,
            //    trips = driverTrips
            //};
        }

        public async Task<int> GetIncome(string userId, Guid driverId, string from, string to)
        {
            int totalPrice = 0;
            DateTime fromTime = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
            DateTime toTime = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
            if (fromTime == toTime)
            {
                DateTime startDateTime = fromTime; //Today at 00:00:00
                DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
                foreach (var connectionString in _connectionStrings)
                {
                    using var context = new TripServiceContext(connectionString);
                    List<Models.Trip> trips = await context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToListAsync();
                    foreach (var trip in trips)
                    {
                        if (trip.Price != null)
                        {
                            totalPrice += trip.Price.Value;
                        }
                    }
                }
                return totalPrice;
            }
            toTime = toTime.AddDays(1).AddTicks(-1);
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> trips = await context.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime && t.TripStatus == Catalouge.Trip.Done).ToListAsync();
                foreach (var trip in trips)
                {
                    if (trip.Price != null)
                    {
                        totalPrice += trip.Price.Value;
                    }
                }
            }
            return totalPrice;

            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //int totalPrice = 0;
            //DateTime fromTime = DateTime.ParseExact(from, "d/M/yyyy", CultureInfo.InvariantCulture);
            //DateTime toTime = DateTime.ParseExact(to, "d/M/yyyy", CultureInfo.InvariantCulture);
            //if (fromTime == toTime)
            //{
            //    DateTime startDateTime = fromTime; //Today at 00:00:00
            //    DateTime endDateTime = toTime.AddDays(1).AddTicks(-1); //Today at 23:59:59
            //    List<Models.Trip> driverTodayTrips = dbContext.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= startDateTime && t.CompleteTime <= endDateTime && t.TripStatus == Catalouge.Trip.Done).ToList();
            //    foreach (var trip in driverTodayTrips)
            //    {
            //        if (trip.Price != null)
            //        {
            //            totalPrice += trip.Price.Value;
            //        }
            //    }
            //    return totalPrice;
            //}
            //toTime = toTime.AddDays(1).AddTicks(-1);
            //List<Models.Trip> driverTrips = dbContext.Trip.Where(t => t.DriverId == driverId && t.CompleteTime >= fromTime && t.CompleteTime <= toTime && t.TripStatus == Catalouge.Trip.Done).ToList();
            //foreach (var trip in driverTrips)
            //{
            //    if (trip.Price != null)
            //    {
            //        totalPrice += trip.Price.Value;
            //    }
            //}

            //return totalPrice;
        }

        public async Task<int> CompleteTrip(string userId, Guid tripId)
        {
            string currentConnectionString = "";
            Models.Trip findTrip = new Models.Trip();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                findTrip = await context.Trip.FindAsync(tripId);
                if (findTrip != null)
                {
                    currentConnectionString = connectionString;
                    break;
                }
            }
            using var dbContext = new TripServiceContext(currentConnectionString);
            Models.Trip trip = await dbContext.Trip.FindAsync(tripId);
            if (trip == null || trip.TripStatus == Catalouge.Trip.Done)
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
            return await dbContext.SaveChangesAsync();
        }

        //public async Task<List<Models.Trip>> GetTrips()
        //{
        //    return await context.Trip.ToListAsync();
        //}



        public async Task<int> CalcNumOfPagesForPassenger(string userId, Guid passengerId, int pageSize)
        {
            int totalRecords = 0;
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                totalRecords += await context.Trip.Where(t => t.PassengerId == passengerId).CountAsync();
            }
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //int totalRecords = await dbContext.Trip.Where(t => t.PassengerId == passengerId).CountAsync();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<int> CalcNumOfPagesForDriver(string userId, Guid driverId, int pageSize)
        {
            int totalRecords = 0;
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                totalRecords += await context.Trip.Where(t => t.DriverId == driverId).CountAsync();
            }
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //int totalRecords = await dbContext.Trip.Where(t => t.DriverId == driverId).CountAsync();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<List<Models.Trip>> GetPassengerTripsPaging(string userId, Guid passengerId, int pageSize, int pageNum)
        {
            List<Models.Trip> tripList = new List<Models.Trip>();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> temp = await context.Trip
                            .Where(p => p.PassengerId == passengerId)
                            .OrderBy(p => p.CreatedTime)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                tripList.AddRange(temp);
            }
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //var passengerTrips = dbContext.Trip
            //                .Where(p => p.PassengerId == passengerId)
            //                .OrderBy(p => p.CreatedTime)
            //                .Skip((pageNum - 1) * pageSize)
            //                .Take(pageSize)
            //                .ToList();
            return tripList;
        }

        public async Task<List<Models.Trip>> GetDriverTripsPaging(string userId, Guid driverId, int pageSize, int pageNum)
        {
            List<Models.Trip> tripList = new List<Models.Trip>();
            foreach (var connectionString in _connectionStrings)
            {
                using var context = new TripServiceContext(connectionString);
                List<Models.Trip> temp = await context.Trip
                            .Where(p => p.DriverId == driverId)
                            .OrderBy(p => p.CreatedTime)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                tripList.AddRange(temp);
            }
            //using var dbContext = new TripServiceContext(GetConnectionString(userId));
            //var driverTrips = dbContext.Trip
            //                .Where(p => p.DriverId == driverId)
            //                .OrderBy(p => p.CreatedTime)
            //                .Skip((pageNum - 1) * pageSize)
            //                .Take(pageSize)
            //                .ToList();
            return tripList;
        }


        public async Task<int> ClearTable()
        {
            foreach (var connectionString in _connectionStrings)
            {
                using var dbContext = new TripServiceContext(connectionString);
                dbContext.RemoveRange(dbContext.Trip);
                await dbContext.SaveChangesAsync();
            }
            return 1;
        }
    }
}
