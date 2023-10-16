using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Models
{
    [Table("ChatMessage", Schema = "dbo")]
    public class ChatMessage
    {
        [Key]
        public Guid ChatMessageId { get; set; }

        public Guid TripId { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public DateTime SendTime { get; set; }
        public Guid SenderId { get; set; }
    }
}
