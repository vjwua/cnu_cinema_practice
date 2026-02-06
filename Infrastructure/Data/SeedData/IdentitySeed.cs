using Core.Constants;

namespace Infrastructure.Data.SeedData;

using Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public static class IdentitySeed
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        string defaultAdminEmail,
        string defaultAdminPassword,
        ILogger? logger = null)
    {
        await SeedRolesAsync(roleManager, logger);
        await SeedDefaultAdminAsync(userManager, defaultAdminEmail, defaultAdminPassword, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger? logger)
    {
        foreach (var roleName in new[] { RoleNames.Admin, RoleNames.User })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                logger?.LogInformation("Created role: {RoleName}", roleName);
            else
                logger?.LogWarning("Failed to create role {RoleName}: {Errors}",
                    roleName, string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedDefaultAdminAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        ILogger? logger)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger?.LogWarning("Failed to create default admin: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        result = await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        if (result.Succeeded)
            logger?.LogInformation("Created default admin user: {Email}", email);
        else
            logger?.LogWarning("Created admin user but failed to assign Admin role: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}