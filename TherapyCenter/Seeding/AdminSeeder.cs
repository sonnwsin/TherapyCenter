using Serilog;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;

namespace TherapyCenter.Seeding
{
    /// <summary>
    /// Ensures at least one Admin user exists after the app starts.
    /// Skips if any user already has Role "Admin" (no duplicate admins on restart).
    /// </summary>
    public static class AdminSeeder
    {
        /// <summary>Seeded admin login (JWT / Postman).</summary>
        public const string AdminEmail = "admin@therapy.com";

        /// <summary>Plain text — stored as BCrypt hash in database.</summary>
        public const string AdminPassword = "123456";

        public static async Task EnsureDefaultAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var existingAdmins = await users.GetByRoleAsync("Admin").ConfigureAwait(false);
            if (existingAdmins.Count > 0)
                return;

            if (await users.GetByEmailAsync(AdminEmail).ConfigureAwait(false) != null)
                return;

            var admin = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                Role = "Admin",
                PhoneNumber = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await users.AddUserAsync(admin).ConfigureAwait(false);
            Log.Information("AdminSeeder: created default admin {Email}.", AdminEmail);
        }
    }
}
