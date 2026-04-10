using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Create Admin role
            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
                logger.LogInformation("Admin role created");
            }

            // Read admin credentials — NO hardcoded fallback password
            var adminEmail = configuration["AdminSettings:DefaultEmail"];
            var adminPassword = configuration["AdminSettings:DefaultPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                logger.LogWarning("AdminSettings:DefaultEmail is not configured. Skipping admin user creation");
                return;
            }

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin is not null)
            {
                if (!await userManager.IsInRoleAsync(existingAdmin, AdminRole))
                {
                    await userManager.AddToRoleAsync(existingAdmin, AdminRole);
                    logger.LogInformation("Existing admin user added to role {Role}: {Email}", AdminRole, adminEmail);
                }
                else
                {
                    logger.LogInformation("Admin user already exists: {Email}", adminEmail);
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword == "CHANGE_ME_ON_DEPLOY")
            {
                logger.LogWarning("AdminSettings:DefaultPassword is not configured or is the placeholder value. Skipping admin user creation. Set this via environment variable for production");
                return;
            }

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
                logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }
}
