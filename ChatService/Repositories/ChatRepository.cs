using ChatService.DTOs;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Repositories
{
    public class ChatRepository : BaseRepository
    {
        private readonly FireStoreService _storeService;
        public ChatRepository(ChatDbContext context) : base(context)
        {
            _storeService = new FireStoreService();
        }

        public async Task<int> StoreChat(string tripId)
        {
            ChatResponseDTO chatResponseDTO = await GetChatFromFireStore(tripId);
            if(chatResponseDTO.Messages == null)
            {
                return 0;
            }
            Chat chat = new Chat()
            {
                TripId = Guid.Parse(tripId),
                DriverId = chatResponseDTO.DriverId,
                PassengerId = chatResponseDTO.PassengerId,
                TripCreatedTime = chatResponseDTO.TripCreatedTime,
            };
            List<ChatMessage> chatMessages = new List<ChatMessage>();
            chatMessages = chatResponseDTO.Messages;
            await context.Chat.AddAsync(chat);
            foreach(var message in chatMessages)
            {
                ChatMessage chatMessage = new ChatMessage()
                {
                    ChatMessageId = Guid.NewGuid(),
                    Message = message.Message,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    SendTime = message.SendTime,
                    TripId = Guid.Parse(tripId),
                };
                await context.ChatMessage.AddAsync(chatMessage);
            }

            return await context.SaveChangesAsync();
        }

        public async Task<ChatResponseDTO> GetChat(string tripId)
        {
            Chat chat = await context.Chat.FindAsync(Guid.Parse(tripId));
            if (chat == null)
            {
                ChatResponseDTO chatResponseDTO1 = new ChatResponseDTO();
                chatResponseDTO1.Messages = new List<ChatMessage>();
                return chatResponseDTO1;
            }
            List<ChatMessage> messages = await context.ChatMessage.Where(m => m.TripId == Guid.Parse(tripId) ).ToListAsync();
            ChatResponseDTO chatResponseDTO = new ChatResponseDTO()
            {
                DriverId = chat.DriverId,
                PassengerId = chat.PassengerId,
                TripId = chat.TripId,
                Messages = messages,
                TripCreatedTime = chat.TripCreatedTime
            };
            return chatResponseDTO;
        }

        public async Task<ChatResponseDTO> GetChatFromFireStore(string tripId)
        {
            ChatResponseDTO chatResponse = await _storeService.GetTrip(tripId);
            Console.WriteLine(chatResponse);
            
            return chatResponse;
        }

        public async Task<int> ClearTable()
        {
            context.RemoveRange(context.Chat);
            return await context.SaveChangesAsync();
        }
    }
}
