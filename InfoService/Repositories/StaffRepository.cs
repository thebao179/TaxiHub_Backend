using InfoService.Models;
using Microsoft.EntityFrameworkCore;

namespace InfoService.Repositories
{
    public class StaffRepository : BaseRepository
    {
        public StaffRepository(InfoDbContext context) : base(context)
        {
        }

        public async Task<List<Staff>> GetAllStaffs()
        {
            return await context.Staff.ToListAsync();
        }

        public async Task<int> CalcNumOfPages(int pageSize)
        {
            int totalRecords = context.Staff.Count();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<List<Staff>> GetStaffsWithPagination(int pageNum, int pageSize)
        {
            var staffs = context.Staff
                            .OrderBy(p => p.AccountNum)
                            .Skip((pageNum - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return staffs;
        }

        public async Task<int> AddStaffInfo(Staff staff)
        {
            await context.Staff.AddAsync(staff);
            return await context.SaveChangesAsync();
        }

        public async Task<Staff> GetStaffById(Guid AccountId)
        {
            return await context.Staff.FindAsync(AccountId);
        }

        // -4 email already exist
        // -3 phone already exist
        public async Task<int> UpdateStaffInfo(Staff staff)
        {
            Staff stf = await context.Staff.FindAsync(staff.AccountId);
            if (context.Staff.Where(p => p.Phone == staff.Phone).Any() == true)
            {
                if(stf.Phone != staff.Phone)
                {
                    return -3;
                }
            }
            if (context.Staff.Where(p => p.Email == staff.Email).Any() == true)
            {
                if(stf.Email != staff.Email)
                {
                    return -4;
                }
            }
            stf.Phone = staff.Phone;
            stf.Email = staff.Email;
            stf.IdentityNumber = staff.IdentityNumber;
            stf.Name = staff.Name;
            stf.Gender = staff.Gender;
            stf.Address = staff.Address;
            return await context.SaveChangesAsync();
        }

        public async Task<bool> CheckStaffExist(Guid AccountId)
        {
            return await context.Staff.AnyAsync(p => p.AccountId == p.AccountId);
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.Staff);
            return await context.SaveChangesAsync();
        }
    }
}
