using Helper;
using TripService.Models;

namespace TripService.Repositories
{
    public class TripFeedbackRepository : BaseRepository
    {
        public TripFeedbackRepository(TripDbContext context) : base(context)
        {
        }

        public async Task<int> RateTrip(Guid tripId, string description, double rate)
        {
            TripFeedback tripFeedback = context.TripFeedback.FirstOrDefault(t => t.TripId == tripId);
            if (tripFeedback != null)
            {
                return 0;
            }
            TripFeedback feedback = new TripFeedback()
            {
                TripId = tripId,
                Note = description,
                Rate = rate
            };
            Trip trip = await context.Trip.FindAsync(tripId);
            if(trip == null)
            {
                return 0;
            }
            if (trip.TripStatus != Catalouge.Trip.Done)
            {
                return 0;
            }
            await context.TripFeedback.AddAsync(feedback);
            return await context.SaveChangesAsync();
        }

        public async Task<TripFeedback> GetTripFeedback(Guid tripId)
        {
            TripFeedback feedBack = await context.TripFeedback.FindAsync(tripId);
            return feedBack;
        }

        public async Task<int> CalcNumOfPages(int pageSize)
        {
            int totalRecords = context.TripFeedback.Count();
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.TripFeedback);
            return await context.SaveChangesAsync();
        }
    }
}
