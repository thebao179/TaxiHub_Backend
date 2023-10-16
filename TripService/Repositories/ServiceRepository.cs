namespace TripService.Repositories
{
    public class ServiceRepository : DbContextProvider
    {
        TripRepository trip;
        public TripRepository Trip
        {
            get
            {
                if (trip == null)
                {
                    trip = new TripRepository(Context);
                }
                return trip;
            }
        }

        TripFeedbackRepository tripFeedBack;
        public TripFeedbackRepository TripFeedBack
        {
            get
            {
                if (tripFeedBack == null)
                {
                    tripFeedBack = new TripFeedbackRepository(Context);
                }
                return tripFeedBack;
            }
        }

        TripRequestRepository tripRequest;
        public TripRequestRepository TripRequest
        {
            get
            {
                if (tripRequest == null)
                {
                    tripRequest = new TripRequestRepository(Context);
                }
                return tripRequest;
            }
        }
    }
}
