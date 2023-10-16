using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoService.Models
{
    [Table("Vehicle", Schema = "dbo")]
    public class Vehicle
    {
        [Key]
        public Guid VehicleId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleNum { get; set; }

        public Guid DriverId { get; set; }
        public string VehicleType { get; set; } // Khai bao danh muc cho value nay
        public string LicensePlatesNum { get; set; }
        public string? VehicleName { get; set; }
        public string? Brand { get; set; }
    }
}
