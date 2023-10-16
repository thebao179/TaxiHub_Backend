namespace ChatService.Repositories
{
    public class DbContextProvider : IDisposable
    {
        ChatDbContext context;
        protected ChatDbContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new ChatDbContext();
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
