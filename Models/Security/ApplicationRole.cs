using Core_Web.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Core_Web.Models.Security
{
    public class ApplicationRole : IdentityRole<long>, IAuditable
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : base(roleName) { }

        public bool IsActive { get; set; } = true;
        public List<AppRoute> Routes { get; set; } = [];

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

}
