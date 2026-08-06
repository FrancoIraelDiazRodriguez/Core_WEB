using System;
using System.ComponentModel.DataAnnotations;
using Core_Web.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core_Web.Models.Security
{
    public class AppRoute : IAuditable
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(50)] 
        public required string Module { get; set; }   // "user"
        [Required, StringLength(50)] 
        public required string Action { get; set; }   // "index"
        [StringLength(100)] 
        public string? MenuModule { get; set; }
        public bool IsActive { get; set; } = true;

        public List<ApplicationRole> Roles { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
