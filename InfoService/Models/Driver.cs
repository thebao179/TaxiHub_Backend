using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoService.Models
{
    [Table("Driver", Schema = "dbo")]
    public class Driver
    {
        [Key]
        public Guid AccountId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountNum { get; set; }

        public string? IdentityNumber { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool? Gender { get; set; }
        public bool? HaveVehicleRegistered { get; set; }
        public string? Address { get; set; }
        public double? AverageRate { get; set; }
        public int? NumberOfRate { get; set; }
        public int? NumberOfTrip { get; set; }
    }
}
