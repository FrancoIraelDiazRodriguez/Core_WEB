using System.ComponentModel.DataAnnotations;

namespace Core_Web.Models.Security;

public class Log
{
    public long Id { get; set; }

    // ── Quién ──────────────────────────────
    public long? UserId { get; set; }              // null = anónimo
    public ApplicationUser? User { get; set; }

    [StringLength(255)]
    public string? UserEmail { get; set; }         // snapshot, sobrevive al borrado

    [StringLength(255)]
    public string? AttemptedIdentifier { get; set; } // email usado en un login fallido

    // ── Qué ────────────────────────────────
    [Required, StringLength(50)]
    public required string Action { get; set; }    // Creado, Actualizado, Eliminado, Login, LoginFallido, Logout

    [StringLength(100)]
    public string? TableName { get; set; }

    [StringLength(50)]
    public string? RecordId { get; set; }

    // ── Desde dónde ────────────────────────
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(255)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
}