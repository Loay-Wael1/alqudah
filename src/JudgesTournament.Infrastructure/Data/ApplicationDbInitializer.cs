using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JudgesTournament.Infrastructure.Data;

public static class ApplicationDbInitializer
{
    private const string AdminRole = "Admin";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

            // Create Admin role
            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
                logger.LogInformation("Admin role created");
            }

            // Create default admin user
            var adminEmail = configuration["AdminSettings:DefaultEmail"] ?? "admin@judgescup.com";
            var adminPassword = configuration["AdminSettings:DefaultPassword"] ?? "Admin@2026!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin is null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                    logger.LogInformation("Default admin user created: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }
}
