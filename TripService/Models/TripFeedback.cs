using System;
using System.ComponentModel.DataAnnotations;

namespace TripService.Models
{
    public class TripFeedback
    {
        [Key]
        public Guid TripId { get; set; }
        public string? Note { get; set; }
        public double? Rate { get; set; }
    }
}
