using Core_Web.Data;
using Core_Web.Data.Seeders;
using Core_Web.Models.Security;
using Core_Web.Security;
using Core_Web.Services.Implementations;
using Core_Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new PermissionRequirement())
        .Build());
// 1. Conexión a BD

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<CoreContext>(options =>
{
    var provider = builder.Configuration["DatabaseProvider"]; // ya existe en appsettings

    if(provider == "InMemory")
        options.UseInMemoryDatabase("CoreWebDev");
    else
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Tu regla StrongPassword de Laravel:
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.User.RequireUniqueEmail = true;   // tu 'unique:users'

    // Esto Laravel no te lo daba: bloqueo por intentos fallidos
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddRoles<ApplicationRole>()             // ← tu clase, no IdentityRole
    .AddEntityFrameworkStores<CoreContext>()
    .AddSignInManager()                      // para el login con lockout
    .AddDefaultTokenProviders();

// 3. JWT: quién valida el token en cada petición
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();   // ← ¿quién eres?  (faltaba)
app.UseAuthorization();    // ← ¿puedes?

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
