using System.Security.Claims;
using TherapyCenter.DTOs.Appointment;
using TherapyCenter.Exceptions;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IBookingValidationService _bookingValidation;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            ISlotRepository slotRepository,
            IPatientRepository patientRepository,
            IHttpContextAccessor httpContextAccessor,
            IDoctorRepository doctorRepository,
            IBookingValidationService bookingValidation)
        {
            _appointmentRepository = appointmentRepository;
            _slotRepository = slotRepository;
            _patientRepository = patientRepository;
            _httpContextAccessor = httpContextAccessor;
            _doctorRepository = doctorRepository;
            _bookingValidation = bookingValidation;
        }

        public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
        {
            var user = GetCurrentUserOrThrow();

            if (user.IsInRole("Guardian"))
            {
                var guardianId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
                if (patient == null)
                    throw new Exception("Patient not found");

                if (patient.GuardianId != guardianId)
                    throw new ForbiddenException("You cannot book appointments for this patient.");
            }

            var slot = await _slotRepository.GetByIdAsync(dto.SlotId);

            if (slot == null)
                throw new Exception("Slot not found");

            if (slot.IsBooked)
                throw new Exception("Slot already booked");

            if (slot.DoctorId != dto.DoctorId)
                throw new Exception("Slot does not belong to the selected doctor.");

            await _bookingValidation.ValidateBookableWindowAsync(
                dto.DoctorId,
                dto.TherapyId,
                slot.Date,
                slot.StartTime,
                slot.EndTime);

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                TherapyId = dto.TherapyId,
                ReceptionistId = dto.ReceptionistId,
                AppointmentDate = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Status = "Scheduled",
                Notes = dto.Notes
            };

            var created = await _appointmentRepository.AddAsync(appointment);

            slot.IsBooked = true;
            await _slotRepository.UpdateAsync(slot);

            return Map(created);
        }

        public async Task<List<AppointmentResponseDto>> GetAllAsync()
        {
            var list = await _appointmentRepository.GetAllAsync();
            return list.Select(Map).ToList();
        }

        public async Task<AppointmentResponseDto> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            await EnsureCanViewAppointmentAsync(appointment);

            return Map(appointment);
        }

        public async Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid appointment id.");

            if (dto == null)
                throw new ArgumentException("Request body is required.");

            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Admin") && !user.IsInRole("Receptionist"))
                throw new ForbiddenException("You are not allowed to update appointments.");

            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Notes = dto.Notes;

            await _appointmentRepository.UpdateAsync(appointment);

            return Map(appointment);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Admin"))
                throw new ForbiddenException("Only administrators can delete appointments.");

            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            await _appointmentRepository.DeleteAsync(appointment);
        }

        public async Task CompleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Doctor"))
                throw new ForbiddenException("Only doctors can complete appointments.");

            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            var doctorUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorRepository.GetByUserIdAsync(doctorUserId);
            if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                throw new ForbiddenException("You can only complete appointments assigned to you.");

            appointment.Status = "Completed";

            await _appointmentRepository.UpdateAsync(appointment);
        }

        public async Task CancelAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var user = GetCurrentUserOrThrow();
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            if (user.IsInRole("Admin") || user.IsInRole("Receptionist"))
            {
                appointment.Status = "Cancelled";
                await _appointmentRepository.UpdateAsync(appointment);
                return;
            }

            if (user.IsInRole("Doctor"))
            {
                var doctorUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var doctor = await _doctorRepository.GetByUserIdAsync(doctorUserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                    throw new ForbiddenException("You can only cancel appointments assigned to you.");

                appointment.Status = "Cancelled";
                await _appointmentRepository.UpdateAsync(appointment);
                return;
            }

            throw new ForbiddenException("You are not allowed to cancel this appointment.");
        }

        private static AppointmentResponseDto Map(Appointment a)
        {
            return new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                TherapyId = a.TherapyId,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes
            };
        }

        public async Task<AppointmentResponseDto> CreateWalkInAsync(CreateWalkInAppointmentDto dto)
        {
            await _bookingValidation.ValidateBookableWindowAsync(
                dto.DoctorId,
                dto.TherapyId,
                dto.Date,
                dto.StartTime,
                dto.EndTime);

            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.Now
            };

            await _patientRepository.AddAsync(patient);

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = dto.DoctorId,
                TherapyId = dto.TherapyId,
                ReceptionistId = null,
                AppointmentDate = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = "Scheduled",
                Notes = dto.Notes
            };

            var created = await _appointmentRepository.AddAsync(appointment);

            return Map(created);
        }

        public async Task<List<AppointmentResponseDto>> GetByDoctorAsync(int doctorId)
        {
            var list = await _appointmentRepository.GetAllAsync();

            return list
                .Where(a => a.DoctorId == doctorId)
                .Select(Map)
                .ToList();
        }

        public async Task<AppointmentResponseDto> BookOnlineAsync(BookOnlineAppointmentDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new Exception("User not authenticated");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new Exception("Invalid token");

            var userId = int.Parse(userIdClaim.Value);

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                throw new Exception("First name is required");

            await _bookingValidation.ValidateBookableWindowAsync(
                dto.DoctorId,
                dto.TherapyId,
                dto.AppointmentDate,
                dto.StartTime,
                dto.EndTime);

            var patient = new Patient
            {
                GuardianId = userId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth.HasValue
                    ? dto.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                Gender = dto.Gender,
                MedicalHistory = dto.MedicalHistory,
                CreatedAt = DateTime.UtcNow
            };

            var createdPatient = await _patientRepository.AddAsync(patient);

            var appointment = new Appointment
            {
                PatientId = createdPatient.PatientId,
                DoctorId = dto.DoctorId,
                TherapyId = dto.TherapyId,
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ReceptionistId = null,
                Status = "Scheduled",
                Notes = "Online Booking",
                CreatedAt = DateTime.UtcNow
            };

            var createdAppointment = await _appointmentRepository.AddAsync(appointment);

            return Map(createdAppointment);
        }

        public async Task<List<AppointmentResponseDto>> GetAppointmentsForGuardianAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new Exception("User not authenticated");

            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new Exception("UserId not found in token");

            var guardianId = int.Parse(claim.Value);

            var patients = await _patientRepository.GetByGuardianIdAsync(guardianId);

            if (patients == null || !patients.Any())
                return new List<AppointmentResponseDto>();

            var patientIds = patients.Select(p => p.PatientId).ToList();

            var appointments = await _appointmentRepository.GetAllAsync();

            var result = appointments
                .Where(a => patientIds.Contains(a.PatientId))
                .Select(Map)
                .ToList();

            return result;
        }

        private ClaimsPrincipal GetCurrentUserOrThrow()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Authentication required.");

            return user;
        }

        private async Task EnsureCanViewAppointmentAsync(Appointment appointment)
        {
            var user = GetCurrentUserOrThrow();

            if (user.IsInRole("Admin"))
                return;

            if (user.IsInRole("Receptionist"))
                return;

            if (user.IsInRole("Doctor"))
            {
                var doctorUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var doctor = await _doctorRepository.GetByUserIdAsync(doctorUserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                    throw new ForbiddenException("You do not have access to this appointment.");

                return;
            }

            if (user.IsInRole("Guardian"))
            {
                var guardianId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (appointment.Patient?.GuardianId != guardianId)
                    throw new ForbiddenException("You do not have access to this appointment.");

                return;
            }

            if (user.IsInRole("Patient"))
            {
                throw new ForbiddenException("You do not have access to this appointment.");
            }

            throw new ForbiddenException("You do not have access to this appointment.");
        }
    }
}
