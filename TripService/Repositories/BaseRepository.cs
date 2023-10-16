namespace TripService.Repositories
{
    public abstract class BaseRepository
    {
        protected TripDbContext context;
        public BaseRepository(TripDbContext context)
        {
            this.context = context;
        }
    }
}
