using System;
using System.ComponentModel.DataAnnotations;

namespace TripService.Models
{
    public class Trip
    {
        [Key]
        public Guid TripId { get; set; }
        public Guid? DriverId { get; set; }
        public Guid? PassengerId { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? VehicleId { get; set; }
        public DateTime? CompleteTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? Destination { get; set; }
        public string? PassengerPhone { get; set; }
        public double? LatDesAddr { get; set; }
        public double? LongDesAddr { get; set; }
        public string? StartAddress { get; set; }
        public double? LatStartAddr { get; set; }
        public double? LongStartAddr { get; set; }
        public string? TripStatus { get; set; } // Khai bao danh muc cho value nay
        public double? Distance { get; set; }
        public int? Price { get; set; }
        public string? VehicleType { get; set; } // Danh muc khai bao trong class Vehicle
        public int? TimeSecond { get; set; }
        public Guid? RequestId { get; set; }
    }
}
