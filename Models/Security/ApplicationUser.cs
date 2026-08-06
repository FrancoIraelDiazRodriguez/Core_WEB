using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core_Web.Interfaces;

namespace Core_Web.Models.Security
{
    public class ApplicationUser : IdentityUser<long>, IAuditable
    {
        [PersonalData]
        [Required, StringLength(50)]
        public required String FirstName { get; set; }
        [PersonalData]
        [Required, StringLength(50)]
        public required String LastName { get; set; }
        [PersonalData, StringLength(255)]
        public String? IdentityNumber { get; set; }
        [PersonalData, AllowNull, StringLength(255)]
        public String? Address { get; set; }
        public bool IsActive { get; set; }
        [DataType(DataType.Date)]
        public DateTime? PasswordExpireAt { get; set; }

        public List<Log> Logs { get; set; } = [];  

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
