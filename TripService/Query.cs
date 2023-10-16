using HotChocolate.Authorization;
using TripService.DataAccess;
using TripService.Models;
using TripService.Repositories;

namespace TripService
{
    public class Query
    {
        private readonly ServiceRepository _repository;
        private readonly TripDataAccess _dataAccess;
        public Query() { 
            _repository = new ServiceRepository();
            _dataAccess = new TripDataAccess();
        }
        public async Task<List<Trip>> GetAllTrips()
        {
            return await _repository.Trip.GetTrips();
        }
        public async Task<List<Trip>> GetTripByDriverId(string driverId)
        {
            return await _dataAccess.GetListTripsByDriver(driverId, Guid.Parse(driverId));
            //return await _repository.Trip.GetListTripsByDriver(Guid.Parse(driverId));
        }

        public async Task<List<Trip>> GetTripByPassengerId(string passengerId)
        {
            return await _dataAccess.GetListTripsByPassenger(passengerId, Guid.Parse(passengerId));
            //return await _repository.Trip.GetListTripsByPassenger(Guid.Parse(passengerId));
        }

    }
}
