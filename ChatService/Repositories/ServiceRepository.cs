namespace ChatService.Repositories
{
    public class ServiceRepository:DbContextProvider
    {
        ChatRepository chat;
        public ChatRepository Chat
        {
            get
            {
                if (chat == null)
                {
                    chat = new ChatRepository(Context);
                }
                return chat;
            }
        }

        ChatMessageRepository chatMessage;
        public ChatMessageRepository ChatMessage
        {
            get
            {
                if (chatMessage == null)
                {
                    chatMessage = new ChatMessageRepository(Context);
                }
                return chatMessage;
            }
        }
    }
}
