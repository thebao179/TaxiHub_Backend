namespace ChatService.Repositories
{
    public class ChatMessageRepository : BaseRepository
    {
        public ChatMessageRepository(ChatDbContext context) : base(context)
        {
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.ChatMessage);
            return await context.SaveChangesAsync();
        }
    }
}
