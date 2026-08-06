using Core_Web.Models.Security;
using Microsoft.AspNetCore.Identity;

namespace Core_Web.Data.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new ApplicationRole("Admin")
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            if (await userManager.FindByEmailAsync("admin@coreweb.com") is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@coreweb.com",
                    Email = "admin@coreweb.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Sistema",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // CreateAsync hashea la contraseña — nunca la guardes tú
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
