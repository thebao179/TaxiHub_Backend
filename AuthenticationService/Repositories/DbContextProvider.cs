namespace AuthenticationService.Repositories
{
    public class DbContextProvider : IDisposable
    {
        AuthenticationDbContext context;
        protected AuthenticationDbContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new AuthenticationDbContext();
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
