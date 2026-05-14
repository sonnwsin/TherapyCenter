using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>Creates ten bookable slots across doctors and dates (aligned with demo appointments).</summary>
    public static class SlotSeeder
    {
        public static async Task<List<Slot>> SeedAsync(
            AppDbContext db,
            IReadOnlyList<Doctor> doctors,
            CancellationToken cancellationToken = default)
        {
            if (doctors.Count < 5)
                throw new InvalidOperationException("Expected five doctors.");

            var baseDate = new DateOnly(2026, 6, 16);
            var slots = new List<Slot>
            {
                new() { DoctorId = doctors[0].DoctorId, Date = baseDate, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 45), IsBooked = false },
                new() { DoctorId = doctors[1].DoctorId, Date = baseDate, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10, 50), IsBooked = false },
                new() { DoctorId = doctors[2].DoctorId, Date = baseDate.AddDays(1), StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 0), IsBooked = false },
                new() { DoctorId = doctors[3].DoctorId, Date = baseDate.AddDays(1), StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(14, 45), IsBooked = false },
                new() { DoctorId = doctors[4].DoctorId, Date = baseDate.AddDays(2), StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(10, 15), IsBooked = false },
                new() { DoctorId = doctors[0].DoctorId, Date = baseDate.AddDays(2), StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(13, 45), IsBooked = false },
                new() { DoctorId = doctors[1].DoctorId, Date = baseDate.AddDays(3), StartTime = new TimeOnly(10, 30), EndTime = new TimeOnly(11, 20), IsBooked = false },
                new() { DoctorId = doctors[2].DoctorId, Date = baseDate.AddDays(3), StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(15, 45), IsBooked = false },
                new() { DoctorId = doctors[3].DoctorId, Date = baseDate.AddDays(4), StartTime = new TimeOnly(8, 30), EndTime = new TimeOnly(9, 15), IsBooked = false },
                new() { DoctorId = doctors[4].DoctorId, Date = baseDate.AddDays(4), StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(12, 50), IsBooked = false }
            };

            await db.Slots.AddRangeAsync(slots, cancellationToken).ConfigureAwait(false);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return slots;
        }
    }
}
