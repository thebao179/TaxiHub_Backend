using System;
using System.ComponentModel.DataAnnotations;

namespace TripService.Models
{
    public class TripRequest
    {
        [Key]
        public Guid RequestId { get; set; }
        public Guid PassengerId { get; set; }
        public Guid StaffId { get; set; }
        public string? RequestStatus { get; set; } // khai bao danh muc cho value nay
        public string? Destination { get; set; }
        public double? LatDesAddr { get; set; }
        public double? LongDesAddr { get; set; }
        public string? StartAddress { get; set; }
        public double? LatStartAddr { get; set; }
        public double? LongStartAddr { get; set; }
        public string? PassengerPhone { get; set; }
        public string? PassengerNote { get; set; }
        public double? Distance { get; set; }
        public string? VehicleType { get; set; }
        public int? Price { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
