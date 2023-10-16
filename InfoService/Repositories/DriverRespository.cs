using InfoService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoService.Repositories
{
    public class DriverRespository : BaseRepository
    {
        public DriverRespository(InfoDbContext context) : base(context)
        {
        }

        public async Task<int> CalcNumOfPages(int pageSize)
        {
            int totalRecords = context.Driver.Count();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<List<Driver>> GetDriversWithPagination(int pageNum, int pageSize)
        {
            var drivers = context.Driver
                            .OrderBy(p => p.AccountNum)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return drivers;
        }

        public async Task<int> AddDriverInfo(Driver driver)
        {
            await context.Driver.AddAsync(driver);
            return await context.SaveChangesAsync();
        }

        // -4 email already exist
        // -3 phone already exist
        public async Task<int> UpdateDriverInfo(Driver driver)
        {
            Driver dri = await context.Driver.FindAsync(driver.AccountId);
            if(context.Driver.Where(p => p.Phone == driver.Phone).Any() == true)
            {
                if(dri.Phone != driver.Phone)
                {
                    return -3;
                }
            }
            if(context.Driver.Where(p => p.Email == driver.Email).Any() == true)
            {
                if(dri.Email != driver.Email)
                {
                    return -4;
                }
            }
            dri.Phone = driver.Phone;
            dri.Email = driver.Email;
            dri.IdentityNumber = driver.IdentityNumber;
            dri.Name = driver.Name;
            dri.Gender = driver.Gender;
            dri.Address = driver.Address;
            return await context.SaveChangesAsync();
        }
        public async Task<bool> CheckDriverExist(Guid AccountId)
        {
            return await context.Driver.AnyAsync(p => p.AccountId == p.AccountId);
        }

        public async Task<List<Driver>> GetAllDrivers()
        {
            return await context.Driver.ToListAsync();
        }

        public async Task<Driver> GetDriverById(Guid AccountId)
        {
            return await context.Driver.FindAsync(AccountId);
        }

        public async Task<int> MarkAlreadyRegisVehicle(Guid driverId)
        {
            Driver driver = await context.Driver.FindAsync(driverId);
            if(driver != null)
            {
                driver.HaveVehicleRegistered = true;
                return await context.SaveChangesAsync();
            }
            return -1;
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.Driver);
            return await context.SaveChangesAsync();
        }

    }
}
