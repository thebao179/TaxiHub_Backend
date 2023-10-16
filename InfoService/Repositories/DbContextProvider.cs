using System;

namespace InfoService.Repositories
{
    public class DbContextProvider : IDisposable
    {
        InfoDbContext context;
        protected InfoDbContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new InfoDbContext();
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
