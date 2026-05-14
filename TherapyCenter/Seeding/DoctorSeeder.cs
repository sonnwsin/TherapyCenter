using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>Creates five doctor user accounts and linked <see cref="Doctor"/> profiles.</summary>
    public static class DoctorSeeder
    {
        public static readonly string[] DoctorEmails =
        {
            "doctor1@test.com",
            "doctor2@test.com",
            "doctor3@test.com",
            "doctor4@test.com",
            "doctor5@test.com"
        };

        public static async Task<List<Doctor>> SeedAsync(AppDbContext db, string passwordHash, CancellationToken cancellationToken = default)
        {
            var specs = new[]
            {
                ("Dr. Vikram", "Shetty", "Pediatric Speech & Language", "Mon,Tue,Wed", new TimeOnly(9, 0), new TimeOnly(13, 0)),
                ("Dr. Neha", "Joshi", "Occupational Therapy", "Tue,Wed,Thu", new TimeOnly(10, 0), new TimeOnly(15, 0)),
                ("Dr. Rahul", "Bose", "Behavioral & Developmental", "Mon,Wed,Fri", new TimeOnly(9, 30), new TimeOnly(14, 30)),
                ("Dr. Anita", "Das", "Physiotherapy", "Mon,Tue,Thu,Fri", new TimeOnly(8, 0), new TimeOnly(12, 0)),
                ("Dr. Sanjay", "Kapoor", "Cognitive Rehabilitation", "Wed,Thu,Fri", new TimeOnly(11, 0), new TimeOnly(17, 0))
            };

            var doctors = new List<Doctor>();

            for (var i = 0; i < 5; i++)
            {
                var (fn, ln, spec, days, st, et) = specs[i];
                var user = new User
                {
                    FirstName = fn,
                    LastName = ln,
                    Email = DoctorEmails[i],
                    PasswordHash = passwordHash,
                    Role = "Doctor",
                    PhoneNumber = $"+9198777{i:D2}01",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await db.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var doctor = new Doctor
                {
                    UserId = user.UserId,
                    Specialization = spec,
                    Bio = $"Experienced clinician — {spec}.",
                    AvailableDays = days,
                    StartTime = st,
                    EndTime = et
                };
                await db.Doctors.AddAsync(doctor, cancellationToken).ConfigureAwait(false);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                doctors.Add(doctor);
            }

            return doctors;
        }
    }
}
