using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace AuthenticationService.Models
{
    public class AuthenticationInfo
    {
        [Key]
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsThirdPartyAccount { get; set; }
        public string Role { get; set; } // Khai bao danh muc cho value nay
        public string? ValidateEmailString { get; set; }
        public string? ResetPasswordString { get; set; }    
        public bool? IsValidated { get; set; }
        public string? RefreshToken { get; set; }
        public string? PasswordSalt { get; set; }
        public DateTime? RefreshTokenExpiredDate { get; set; }
    }
}
