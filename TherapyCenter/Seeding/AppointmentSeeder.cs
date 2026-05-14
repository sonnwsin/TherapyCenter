using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;

namespace TherapyCenter.Seeding
{
    /// <summary>
    /// Seeds appointments (aligned to demo slots), updates slot <see cref="Slot.IsBooked"/>,
    /// optional <see cref="Payment"/> and <see cref="DoctorFinding"/> rows for API/report tests.
    /// </summary>
    public static class AppointmentSeeder
    {
        public static async Task SeedAsync(
            AppDbContext db,
            User receptionist,
            IReadOnlyList<Patient> patients,
            IReadOnlyList<Therapy> therapies,
            IReadOnlyList<Slot> slots,
            CancellationToken cancellationToken = default)
        {
            if (patients.Count < 5 || therapies.Count < 6 || slots.Count < 10)
                throw new InvalidOperationException("Demo seed prerequisites not met.");

            var statuses = new[]
            {
                "Completed",
                "Completed",
                "Completed",
                "Completed",
                "Scheduled",
                "Scheduled",
                "Scheduled",
                "Cancelled",
                "Scheduled",
                "Scheduled"
            };

            var appointments = new List<Appointment>();

            for (var i = 0; i < 10; i++)
            {
                var slot = slots[i];
                var therapy = therapies[i % therapies.Count];
                var patient = patients[i % patients.Count];

                var appt = new Appointment
                {
                    PatientId = patient.PatientId,
                    DoctorId = slot.DoctorId,
                    TherapyId = therapy.TherapyId,
                    ReceptionistId = i < 6 ? receptionist.UserId : null,
                    AppointmentDate = slot.Date,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Status = statuses[i],
                    Notes = $"Demo appointment #{i + 1} — seeded.",
                    CreatedAt = DateTime.UtcNow.AddHours(-i)
                };
                appointments.Add(appt);
                await db.Appointments.AddAsync(appt, cancellationToken).ConfigureAwait(false);
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            for (var i = 0; i < slots.Count; i++)
            {
                if (statuses[i] != "Cancelled")
                    slots[i].IsBooked = true;
            }

            db.Slots.UpdateRange(slots);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var therapyById = therapies.ToDictionary(t => t.TherapyId);

            void AddPayment(int index, string status, string? transactionId, DateTime? paidAt)
            {
                var a = appointments[index];
                var amount = therapyById[a.TherapyId].Cost;
                db.Payments.Add(new Payment
                {
                    AppointmentId = a.AppointmentId,
                    Amount = amount,
                    PaymentMethod = "Razorpay",
                    TransactionId = transactionId,
                    Status = status,
                    PaidAt = paidAt,
                    CreatedAt = DateTime.UtcNow
                });
            }

            AddPayment(0, "Paid", "seed_order_paid_001", DateTime.UtcNow.AddDays(-1));
            AddPayment(1, "Pending", "seed_order_pending_002", null);
            AddPayment(3, "Paid", "seed_order_paid_004", DateTime.UtcNow.AddHours(-12));
            AddPayment(4, "Pending", "seed_order_pending_005", null);

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            db.DoctorFindings.AddRange(
                new DoctorFinding
                {
                    AppointmentId = appointments[0].AppointmentId,
                    Observations = "Clear progress on articulation targets.",
                    Recommendations = "Continue home practice 10 min daily.",
                    NextSessionDate = new DateOnly(2026, 6, 23),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new DoctorFinding
                {
                    AppointmentId = appointments[1].AppointmentId,
                    Observations = "Improved seated attention for tabletop tasks.",
                    Recommendations = "Introduce weighted vest trial next visit.",
                    NextSessionDate = new DateOnly(2026, 6, 24),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
