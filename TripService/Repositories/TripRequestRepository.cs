using Helper;
using TripService.FireBaseServices;
using TripService.Models;

namespace TripService.Repositories
{
    public class TripRequestRepository : BaseRepository
    {
        private readonly FirebaseService _fireBaseService;

        public TripRequestRepository(TripDbContext context) : base(context)
        {
            _fireBaseService = new FirebaseService();
        }

        public async Task<int> CreateRequest(TripRequest request)
        {
            await context.TripRequest.AddAsync(request);
            _fireBaseService.AddNewRequest(request);
            return await context.SaveChangesAsync();
        }

        public async Task<int> CancelRequest(Guid requestId)
        {
            TripRequest request = await context.TripRequest.FindAsync(requestId);
            if (request == null || request.RequestStatus == Catalouge.Request.MovedToTrip)
            {
                return 0;
            }
            request.RequestStatus = Catalouge.Request.Canceled;
            _fireBaseService.RemoveRequest(requestId);
            return await context.SaveChangesAsync();
        }

        public object CalcPrice(double distance)
        {
            return new
            {
                Motorbike = distance*1.0*12000,
                Car4S = distance*1.0*15000,
                Car7S = distance*1.0*18000,
            };
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.TripRequest);   
            return await context.SaveChangesAsync();
        }
    }
}
