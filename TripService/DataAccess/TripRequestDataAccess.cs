using Helper;
using System.Security.Cryptography;
using System.Text;
using TripService.FireBaseServices;
using TripService.Models;

namespace TripService.DataAccess
{
    public class TripRequestDataAccess
    {
        private readonly FirebaseService _fireBaseService;
        private readonly List<string> _connectionStrings = new List<string>();

        public TripRequestDataAccess()
        {
            _fireBaseService = new FirebaseService();
            var connectionStrings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString1"));
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString2"));
            _connectionStrings.Add(connectionStrings.GetConnectionString("connectionString3"));
            //foreach (var connectionString in connectionStrings.GetChildren())
            //{

            //}
        }

        private string GetConnectionString(string userId)
        {
            int userid = Guid.Parse(userId).GetHashCode();
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(userid.ToString()));
            var x = BitConverter.ToUInt16(hash, 0) % _connectionStrings.Count;
            return _connectionStrings[x];
        }

        public async Task<int> CreateRequest(string userId, TripRequest request)
        {
            using var dbContext = new TripServiceContext(GetConnectionString(userId));
            await dbContext.TripRequest.AddAsync(request);
            _fireBaseService.AddNewRequest(request);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> CancelRequest(string userId, Guid requestId)
        {
            using var dbContext = new TripServiceContext(GetConnectionString(userId));
            TripRequest request = await dbContext.TripRequest.FindAsync(requestId);
            if (request == null || request.RequestStatus == Catalouge.Request.MovedToTrip)
            {
                return 0;
            }
            request.RequestStatus = Catalouge.Request.Canceled;
            _fireBaseService.RemoveRequest(requestId);
            return await dbContext.SaveChangesAsync();
        }

        public object CalcPrice(double distance)
        {
            return new
            {
                Motorbike = distance * 1.0 * 12000,
                Car4S = distance * 1.0 * 15000,
                Car7S = distance * 1.0 * 18000,
            };
        }

        public async Task<int> ClearTable()
        {
            foreach(var connectionString in  _connectionStrings)
            {
                using var dbContext = new TripServiceContext(connectionString);
                dbContext.RemoveRange(dbContext.TripRequest);
                await dbContext.SaveChangesAsync();
            }
            return 1;
        }
    }
}
