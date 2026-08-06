using Core_Web.Models.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core_Web.Data;

public class CoreContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
{
    private readonly IHttpContextAccessor _http;

    public CoreContext(DbContextOptions<CoreContext> options, IHttpContextAccessor http)
        : base(options)
    {
        _http = http;   // lo usará el logging automático en el siguiente paso
    }

    public DbSet<AppRoute> Routes { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tu belongsToMany(Route) con tabla pivote propia
        builder.Entity<ApplicationRole>()
            .HasMany(r => r.Routes)
            .WithMany(rt => rt.Roles)
            .UsingEntity(j => j.ToTable("permissions"));

        // No puede haber dos rutas iguales
        builder.Entity<AppRoute>()
            .HasIndex(r => new { r.Module, r.Action })
            .IsUnique();

        // Tu 'unique:users,identity_card' — el filtro permite varios NULL
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.IdentityNumber)
            .IsUnique()
            .HasFilter("[IdentityNumber] IS NOT NULL");

        // Borrar un usuario NO borra sus logs
        builder.Entity<Log>()
            .HasOne(l => l.User)
            .WithMany(u => u.Logs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Los logs se consultan casi siempre por fecha
        builder.Entity<Log>()
            .HasIndex(l => l.CreatedAt);

        builder.Entity<RefreshToken>().HasIndex(rt => rt.Token).IsUnique();
    }
}