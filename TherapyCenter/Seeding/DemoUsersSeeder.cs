using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>Seeds receptionist + guardian accounts (password hash passed in).</summary>
    public static class DemoUsersSeeder
    {
        public const string ReceptionEmail = "reception@therapy.com";

        public static readonly string[] GuardianEmails =
        {
            "guardian1@test.com",
            "guardian2@test.com",
            "guardian3@test.com"
        };

        public static async Task<(User Receptionist, List<User> Guardians)> SeedReceptionistAndGuardiansAsync(
            AppDbContext db,
            string passwordHash,
            CancellationToken cancellationToken = default)
        {
            var reception = new User
            {
                FirstName = "Meera",
                LastName = "Nair",
                Email = ReceptionEmail,
                PasswordHash = passwordHash,
                Role = "Receptionist",
                PhoneNumber = "+919811100001",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var g1 = new User
            {
                FirstName = "Sunita",
                LastName = "Iyer",
                Email = GuardianEmails[0],
                PasswordHash = passwordHash,
                Role = "Guardian",
                PhoneNumber = "+919822200001",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var g2 = new User
            {
                FirstName = "Kavita",
                LastName = "Patel",
                Email = GuardianEmails[1],
                PasswordHash = passwordHash,
                Role = "Guardian",
                PhoneNumber = "+919833300002",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var g3 = new User
            {
                FirstName = "Deepa",
                LastName = "Reddy",
                Email = GuardianEmails[2],
                PasswordHash = passwordHash,
                Role = "Guardian",
                PhoneNumber = "+919844400003",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // EF Core AddRangeAsync signature is (IEnumerable<User>, CancellationToken).
            // Passing multiple entities as separate arguments makes the compiler treat this as a params
            // overload where every argument must be User — so cancellationToken becomes CS1503.
            await db.Users.AddRangeAsync(new[] { reception, g1, g2, g3 }, cancellationToken).ConfigureAwait(false);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return (reception, new List<User> { g1, g2, g3 });
        }
    }
}
