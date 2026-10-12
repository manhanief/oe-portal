using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Resto.Api.Data;
using Resto.Api.Entities;

namespace Resto.Api.Auth;

/// <summary>
/// Creates the first admin account when the system has no users yet.
/// This is done in C# (not in a SQL seed script) so the password is hashed properly.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestoDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        try
        {
            if (await db.Users.AnyAsync())
                return;
        }
        catch (Exception ex)
        {
            // Don't stop the whole API if the database is briefly unavailable at startup
            logger.LogError(ex, "Could not check for existing users; skipping admin seeding");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Username) || string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning(
                "No users exist and SeedAdmin is not configured, so nobody can log in. " +
                "Set SeedAdmin:Username and SeedAdmin:Password with dotnet user-secrets.");
            return;
        }

        var admin = new User
        {
            Username = options.Username.Trim().ToLowerInvariant(),
            Role = UserRole.Admin
        };
        admin.PasswordHash = hasher.HashPassword(admin, options.Password);

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation("Created the first admin account '{Username}'", admin.Username);
    }
}
