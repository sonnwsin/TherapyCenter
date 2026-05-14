using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>Five pediatric patients linked to three guardian users.</summary>
    public static class PatientSeeder
    {
        public static async Task<List<Patient>> SeedAsync(
            AppDbContext db,
            IReadOnlyList<User> guardians,
            CancellationToken cancellationToken = default)
        {
            if (guardians.Count < 3)
                throw new InvalidOperationException("Expected three guardian users.");

            var g0 = guardians[0].UserId;
            var g1 = guardians[1].UserId;
            var g2 = guardians[2].UserId;

            var patients = new List<Patient>
            {
                new()
                {
                    GuardianId = g0,
                    FirstName = "Arjun",
                    LastName = "Verma",
                    DateOfBirth = new DateTime(2016, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "M",
                    MedicalHistory = "Mild speech delay.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    GuardianId = g0,
                    FirstName = "Isha",
                    LastName = "Verma",
                    DateOfBirth = new DateTime(2019, 8, 11, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "F",
                    MedicalHistory = null,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    GuardianId = g1,
                    FirstName = "Rohan",
                    LastName = "Patel",
                    DateOfBirth = new DateTime(2015, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "M",
                    MedicalHistory = "ASD — OT recommended.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    GuardianId = g1,
                    FirstName = "Anaya",
                    LastName = "Patel",
                    DateOfBirth = new DateTime(2018, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "F",
                    MedicalHistory = null,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    GuardianId = g2,
                    FirstName = "Kabir",
                    LastName = "Reddy",
                    DateOfBirth = new DateTime(2014, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "M",
                    MedicalHistory = "ADHD — school accommodations.",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await db.Patients.AddRangeAsync(patients, cancellationToken).ConfigureAwait(false);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return patients;
        }
    }
}
