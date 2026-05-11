using TherapyCenter.DTOs.Slot;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITherapyRepository _therapyRepository;

        public SlotService(
            ISlotRepository slotRepository,
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            ITherapyRepository therapyRepository)
        {
            _slotRepository = slotRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _therapyRepository = therapyRepository;
        }

        public async Task<SlotResponseDto> CreateSlotAsync(CreateSlotDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Invalid time range");

            var existingSlots = await _slotRepository.GetByDoctorAndDateAsync(dto.DoctorId, dto.Date);

            foreach (var s in existingSlots)
            {
                bool overlap = dto.StartTime < s.EndTime && dto.EndTime > s.StartTime;

                if (overlap)
                    throw new Exception("Slot overlaps with existing slot");
            }

            var slot = new Slot
            {
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsBooked = false
            };

            var created = await _slotRepository.AddAsync(slot);

            return Map(created);
        }

        public async Task<List<SlotResponseDto>> GetAllSlotsAsync()
        {
            var slots = await _slotRepository.GetAllAsync();
            return slots.Select(Map).ToList();
        }

        public async Task<List<SlotResponseDto>> GetSlotsByDoctorAsync(int doctorId, DateOnly date)
        {
            var slots = await _slotRepository.GetByDoctorAndDateAsync(doctorId, date);
            return slots.Select(Map).ToList();
        }

        public async Task<SlotResponseDto> GetSlotByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid slot id.");

            var slot = await _slotRepository.GetByIdAsync(id);

            if (slot == null)
                throw new Exception("Slot not found");

            return Map(slot);
        }

        public async Task<SlotResponseDto> UpdateSlotAsync(int id, UpdateSlotDto dto)
        {
            var slot = await _slotRepository.GetByIdAsync(id);

            if (slot == null)
                throw new Exception("Slot not found");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Invalid time range");

            var existingSlots = await _slotRepository.GetByDoctorAndDateAsync(slot.DoctorId, dto.Date);

            foreach (var s in existingSlots)
            {
                if (s.SlotId == id) continue;

                bool overlap = dto.StartTime < s.EndTime && dto.EndTime > s.StartTime;

                if (overlap)
                    throw new Exception("Slot overlaps with existing slot");
            }

            slot.Date = dto.Date;
            slot.StartTime = dto.StartTime;
            slot.EndTime = dto.EndTime;

            await _slotRepository.UpdateAsync(slot);

            return Map(slot);
        }

        public async Task DeleteSlotAsync(int id)
        {
            var slot = await _slotRepository.GetByIdAsync(id);

            if (slot == null)
                throw new Exception("Slot not found");

            await _slotRepository.DeleteAsync(slot);
        }

        private static SlotResponseDto Map(Slot slot)
        {
            return new SlotResponseDto
            {
                SlotId = slot.SlotId,
                DoctorId = slot.DoctorId,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsBooked = slot.IsBooked
            };
        }

        /// <summary>
        /// Builds aligned candidate windows from doctor hours. When <paramref name="therapyId"/> is null,
        /// uses a 30-minute grid for backward compatibility with existing clients.
        /// </summary>
        public async Task<List<DoctorSlotDto>> GetGeneratedSlotsByDoctorAsync(int doctorId, DateOnly date, int? therapyId = null)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            if (doctor.AvailableDays == null ||
                !DoctorScheduleHelper.IsDoctorAvailableOnDate(doctor.AvailableDays, date))
                return new List<DoctorSlotDto>();

            var start = doctor.StartTime!.Value;
            var end = doctor.EndTime!.Value;

            if (start >= end)
                return new List<DoctorSlotDto>();

            var durationMinutes = 30;
            if (therapyId.HasValue)
            {
                var therapy = await _therapyRepository.GetByIdAsync(therapyId.Value);
                if (therapy == null)
                    throw new Exception("Therapy not found.");

                if (therapy.DurationMinutes <= 0)
                    throw new Exception("Therapy duration is invalid.");

                durationMinutes = therapy.DurationMinutes;
            }

            var slots = new List<DoctorSlotDto>();
            var cursor = start;
            while (true)
            {
                var next = cursor.AddMinutes(durationMinutes);
                if (next > end)
                    break;

                slots.Add(new DoctorSlotDto
                {
                    StartTime = cursor,
                    EndTime = next,
                    Status = "Available"
                });

                cursor = next;
            }

            var appointments = await _appointmentRepository
                .GetByDoctorAndDateAsync(doctorId, date);

            var activeAppointments = appointments
                .Where(a => !AppointmentTimeRangeHelper.IsCancelled(a.Status))
                .ToList();

            foreach (var slot in slots)
            {
                var isBooked = activeAppointments.Any(a =>
                    AppointmentTimeRangeHelper.RangesOverlap(a.StartTime, a.EndTime, slot.StartTime, slot.EndTime));

                if (isBooked)
                    slot.Status = "Booked";
            }

            return slots;
        }
    }
}
