using InfoService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoService.Repositories
{
    public class PassengerRepository : BaseRepository
    {
        public PassengerRepository(InfoDbContext context) : base(context)
        {
        }
        public async Task<List<Passenger>> GetAllPassengers()
        {
            return await context.Passenger.ToListAsync();
        }

        public async Task<int> CalcNumOfPages(int pageSize)
        {
            int totalRecords = context.Passenger.Count();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<List<Passenger>> GetPassengersWithPagination(int pageNum, int pageSize)
        {
            var passengers = context.Passenger
                            .OrderBy(p => p.AccountNum)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return passengers;
        }

        public async Task<int> AddPassengerInfo(Passenger passenger)
        {
            await context.Passenger.AddAsync(passenger);
            return await context.SaveChangesAsync();
        }

        public async Task<Passenger> GetPassengerById(Guid AccountId)
        {
            return await context.Passenger.FindAsync(AccountId);
        }

        // -4 email already exist
        // -3 phone already exist
        public async Task<int> UpdatePassengerInfo(Passenger passenger)
        {
            Passenger pass = await context.Passenger.FindAsync(passenger.AccountId);
            if (context.Passenger.Where(p => p.Phone == passenger.Phone).Any() == true)
            {
                if(pass.Phone != passenger.Phone)
                {
                    return -3;
                }
            }
            if (context.Passenger.Where(p => p.Email == passenger.Email).Any() == true)
            {
                if(pass.Email != passenger.Email)
                {
                    return -4;
                }
            }

            pass.Phone = passenger.Phone;
            pass.Email = passenger.Email;
            pass.Name = passenger.Name;
            pass.Gender = passenger.Gender;
            return await context.SaveChangesAsync();
        }
        public async Task<bool> CheckPassengerExist(Guid AccountId)
        {
            return await context.Passenger.AnyAsync(p => p.AccountId == p.AccountId);
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.Passenger);
            return await context.SaveChangesAsync();
        }

    }
}
