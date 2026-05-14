using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>Adds demo therapies (prices, durations) when invoked by <see cref="DemoDataSeeder"/>.</summary>
    public static class TherapySeeder
    {
        public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
        {
            var therapies = new List<Therapy>
            {
                new()
                {
                    Name = "Speech Therapy",
                    Description = "Language articulation and fluency sessions.",
                    DurationMinutes = 45,
                    Cost = 2200m
                },
                new()
                {
                    Name = "Occupational Therapy",
                    Description = "Fine motor skills, ADL training, and sensory integration.",
                    DurationMinutes = 50,
                    Cost = 1850m
                },
                new()
                {
                    Name = "Behavioral Therapy",
                    Description = "ABA-informed strategies for behavior regulation.",
                    DurationMinutes = 60,
                    Cost = 2000m
                },
                new()
                {
                    Name = "Physiotherapy",
                    Description = "Mobility, posture, and strength rehabilitation.",
                    DurationMinutes = 45,
                    Cost = 1750m
                },
                new()
                {
                    Name = "Cognitive Therapy",
                    Description = "Memory, attention, and executive function support.",
                    DurationMinutes = 50,
                    Cost = 2100m
                },
                new()
                {
                    Name = "Developmental Assessment",
                    Description = "Structured screening and developmental milestone review.",
                    DurationMinutes = 90,
                    Cost = 1500m
                }
            };

            await db.Therapies.AddRangeAsync(therapies, cancellationToken).ConfigureAwait(false);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
