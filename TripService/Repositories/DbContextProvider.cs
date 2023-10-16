namespace TripService.Repositories
{
    public class DbContextProvider : IDisposable
    {
        TripDbContext context;
        protected TripDbContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new TripDbContext();
                }
                context.Database.EnsureCreated();
                return context;
            }
        }

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }
}
