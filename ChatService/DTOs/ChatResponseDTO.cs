using ChatService.Models;

namespace ChatService.DTOs
{
    public class ChatResponseDTO
    {
        public Guid TripId { get; set; }
        public Guid DriverId { get; set; }
        public Guid PassengerId { get; set; }
        public DateTime TripCreatedTime { get; set; }
        public List<ChatMessage>? Messages { get; set; }

    }
}
