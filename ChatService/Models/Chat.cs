using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Models
{
    [Table("Chat", Schema = "dbo")]
    public class Chat
    {
        [Key]
        public Guid TripId { get; set; }
        public Guid DriverId { get; set; }
        public Guid PassengerId { get; set; }
        public DateTime TripCreatedTime { get; set; }
    }
}
