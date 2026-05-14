using Microsoft.EntityFrameworkCore;
using Serilog;
using TherapyCenter.Data;

namespace TherapyCenter.Seeding
{
    /// <summary>
    /// Idempotent demo dataset for local / Postman testing. Runs once when the database has no therapies.
    /// </summary>
    /// <remarks>
    /// <para><b>TEST CREDENTIALS — all passwords:</b> <c>123456</c> (BCrypt-hashed in DB)</para>
    /// <list type="bullet">
    /// <item><b>Admin</b> — <c>admin@therapy.com</c> (also created by <see cref="AdminSeeder"/> if no admin exists)</item>
    /// <item><b>Receptionist</b> — <c>reception@therapy.com</c></item>
    /// <item><b>Guardians</b> — <c>guardian1@test.com</c>, <c>guardian2@test.com</c>, <c>guardian3@test.com</c></item>
    /// <item><b>Doctors</b> — <c>doctor1@test.com</c> … <c>doctor5@test.com</c></item>
    /// </list>
    /// <para>Seeded data: 1 receptionist, 3 guardians, 5 doctors, 6 therapies, 5 patients, 10 slots, 10 appointments,
    /// payments (paid + pending), and two doctor findings. Reports summary reflects these counts.</para>
    /// </remarks>
    public static class DemoDataSeeder
    {
        public const string DemoPasswordPlain = "123456";

        public static async Task SeedIfNeededAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await db.Therapies.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                Log.Information("DemoDataSeeder: skipped — therapies already exist (demo was applied earlier).");
                return;
            }

            Log.Information("DemoDataSeeder: starting full demo dataset (empty therapy catalog detected).");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(DemoPasswordPlain);

            Log.Information("DemoDataSeeder: users — receptionist + 3 guardians.");
            var (receptionist, guardians) = await DemoUsersSeeder.SeedReceptionistAndGuardiansAsync(db, passwordHash, cancellationToken)
                .ConfigureAwait(false);

            Log.Information("DemoDataSeeder: 5 doctor accounts + profiles.");
            var doctors = await DoctorSeeder.SeedAsync(db, passwordHash, cancellationToken).ConfigureAwait(false);

            Log.Information("DemoDataSeeder: 6 therapies with varied prices.");
            await TherapySeeder.SeedAsync(db, cancellationToken).ConfigureAwait(false);

            var therapies = await db.Therapies.OrderBy(t => t.TherapyId).ToListAsync(cancellationToken).ConfigureAwait(false);

            Log.Information("DemoDataSeeder: 5 patients linked to guardians.");
            var patients = await PatientSeeder.SeedAsync(db, guardians, cancellationToken).ConfigureAwait(false);

            Log.Information("DemoDataSeeder: 10 slots.");
            var slots = await SlotSeeder.SeedAsync(db, doctors, cancellationToken).ConfigureAwait(false);

            Log.Information("DemoDataSeeder: 10 appointments + slot booking flags + payments + findings.");
            await AppointmentSeeder.SeedAsync(db, receptionist, patients, therapies, slots, cancellationToken).ConfigureAwait(false);

            Log.Information("DemoDataSeeder: finished. Seeded users use password {Password}.", DemoPasswordPlain);
        }
    }
}
