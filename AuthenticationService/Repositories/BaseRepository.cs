namespace AuthenticationService.Repositories
{
    public abstract class BaseRepository
    {
        protected AuthenticationDbContext context;
        public BaseRepository(AuthenticationDbContext context)
        {
            this.context = context;
        }
    }
}
