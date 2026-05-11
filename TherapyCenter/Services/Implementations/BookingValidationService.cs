using TherapyCenter.Helpers;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class BookingValidationService : IBookingValidationService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ITherapyRepository _therapyRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public BookingValidationService(
            IDoctorRepository doctorRepository,
            ITherapyRepository therapyRepository,
            IAppointmentRepository appointmentRepository)
        {
            _doctorRepository = doctorRepository;
            _therapyRepository = therapyRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task ValidateBookableWindowAsync(
            int doctorId,
            int therapyId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found.");

            if (!doctor.StartTime.HasValue || !doctor.EndTime.HasValue)
                throw new Exception("Doctor schedule hours are not configured.");

            if (string.IsNullOrWhiteSpace(doctor.AvailableDays))
                throw new Exception("Doctor availability days are not configured.");

            if (!DoctorScheduleHelper.IsDoctorAvailableOnDate(doctor.AvailableDays, date))
                throw new Exception("Doctor is not available on the selected date.");

            var therapy = await _therapyRepository.GetByIdAsync(therapyId);
            if (therapy == null)
                throw new Exception("Therapy not found.");

            if (therapy.DurationMinutes <= 0)
                throw new Exception("Therapy duration is invalid.");

            if (startTime >= endTime)
                throw new Exception("Invalid time range.");

            var doctorOpen = doctor.StartTime.Value;
            var doctorClose = doctor.EndTime.Value;

            if (startTime < doctorOpen || endTime > doctorClose)
                throw new Exception("Appointment time falls outside doctor working hours.");

            var durationTicks = TimeSpan.FromMinutes(therapy.DurationMinutes).Ticks;
            var windowTicks = (endTime - startTime).Ticks;

            if (windowTicks != durationTicks)
                throw new Exception($"Appointment length must be exactly {therapy.DurationMinutes} minutes for the selected therapy.");

            var offsetTicks = (startTime - doctorOpen).Ticks;
            if (offsetTicks < 0)
                throw new Exception("Invalid appointment start time.");

            if (offsetTicks % durationTicks != 0)
                throw new Exception(
                    $"Appointment must start on a {therapy.DurationMinutes}-minute boundary from doctor opening time ({doctorOpen:HH:mm}).");

            var sameDay = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, date);
            foreach (var appt in sameDay.Where(a => !AppointmentTimeRangeHelper.IsCancelled(a.Status)))
            {
                if (AppointmentTimeRangeHelper.RangesOverlap(appt.StartTime, appt.EndTime, startTime, endTime))
                    throw new Exception("This time overlaps an existing appointment.");
            }
        }
    }
}
