using System.ComponentModel.DataAnnotations;

namespace Core_Web.Models.Security
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(255)]
        public required string Token { get; set; }      // aleatorio, no un JWT

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }        // null = activo

        [StringLength(45)]
        public string? CreatedByIp { get; set; }
    }
}
