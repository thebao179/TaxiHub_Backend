namespace ChatService.Repositories
{
    public abstract class BaseRepository
    {
        protected ChatDbContext context;
        public BaseRepository(ChatDbContext context)
        {
            this.context = context;
        }
    }
}
