

using InfoService.Models;
using InfoService.Repositories;

namespace InfoService
{
    public class Query
    {
        private readonly ServiceRepository _repository;
        public Query()
        {
            _repository = new ServiceRepository();
        }
        public async Task<List<Driver>> GetAllDrivers()
        {
            return await _repository.Driver.GetAllDrivers();
        }

        public async Task<List<Passenger>> GetAllPassengers()
        {
            return await _repository.Passenger.GetAllPassengers();
        }

        public async Task<Passenger> GetPassengerById(string passengerId)
        {
            return await _repository.Passenger.GetPassengerById(Guid.Parse(passengerId));
        }

        public async Task<Driver> GetDriverById(string driverId)
        {
            return await _repository.Driver.GetDriverById(Guid.Parse(driverId));
        }
        //public async Task<List<Trip>> GetTripByDriverId(string driverId)
        //{
        //    return await _repository.Trip.GetListTripsByDriver(Guid.Parse(driverId));
        //}

        //public async Task<List<Trip>> GetTripByPassengerId(string passengerId)
        //{
        //    return await _repository.Trip.GetListTripsByPassenger(Guid.Parse(passengerId));
        //}

    }
}
