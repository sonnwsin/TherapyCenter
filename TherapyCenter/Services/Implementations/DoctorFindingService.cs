using System.Security.Claims;
using TherapyCenter.DTOs.DoctorFinding;
using TherapyCenter.Exceptions;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class DoctorFindingService : IDoctorFindingService
    {
        private readonly IDoctorFindingRepository _repository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DoctorFindingService(
            IDoctorFindingRepository repository,
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DoctorFindingResponseDto> CreateAsync(CreateDoctorFindingDto dto)
        {
            if (dto.AppointmentId <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId)
                ?? throw new Exception("Appointment not found.");

            await EnsureDoctorOwnsAppointmentForFindingWriteAsync(appointment);

            var existing = await _repository.GetByAppointmentIdAsync(dto.AppointmentId);
            if (existing.Count > 0)
                throw new Exception("A finding already exists for this appointment.");

            var finding = new DoctorFinding
            {
                AppointmentId = dto.AppointmentId,
                Observations = dto.Observations,
                Recommendations = dto.Recommendations,
                NextSessionDate = dto.NextSessionDate,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(finding);

            return Map(created);
        }

        public async Task<List<DoctorFindingResponseDto>> GetAllAsync()
        {
            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Admin") && !user.IsInRole("Receptionist"))
                throw new ForbiddenException("You are not allowed to list all findings.");

            var list = await _repository.GetAllAsync();
            return list.Select(Map).ToList();
        }

        public async Task<DoctorFindingResponseDto> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid finding id.");

            var finding = await _repository.GetByIdAsync(id);
            if (finding == null)
                throw new Exception("Finding not found.");

            var appointment = await _appointmentRepository.GetByIdAsync(finding.AppointmentId)
                ?? throw new Exception("Appointment not found.");

            await EnsureCanViewFindingForAppointmentAsync(appointment);

            return Map(finding);
        }

        public async Task<List<DoctorFindingResponseDto>> GetByAppointmentAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("Invalid appointment id.");

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId)
                ?? throw new Exception("Appointment not found.");

            await EnsureCanViewFindingForAppointmentAsync(appointment);

            var list = await _repository.GetByAppointmentIdAsync(appointmentId);

            if (list == null || !list.Any())
                return new List<DoctorFindingResponseDto>();

            return list.Select(Map).ToList();
        }

        public async Task<DoctorFindingResponseDto> UpdateAsync(int id, UpdateDoctorFindingDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid finding id.");

            if (dto == null)
                throw new ArgumentException("Request body is required.");

            var finding = await _repository.GetByIdAsync(id);
            if (finding == null)
                throw new Exception("Finding not found.");

            var appointment = await _appointmentRepository.GetByIdAsync(finding.AppointmentId)
                ?? throw new Exception("Appointment not found.");

            await EnsureDoctorOwnsAppointmentForFindingWriteAsync(appointment);

            finding.Observations = dto.Observations;
            finding.Recommendations = dto.Recommendations;
            finding.NextSessionDate = dto.NextSessionDate;

            await _repository.UpdateAsync(finding);

            return Map(finding);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid finding id.");

            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Admin"))
                throw new ForbiddenException("Only administrators can delete findings.");

            var finding = await _repository.GetByIdAsync(id);
            if (finding == null)
                throw new Exception("Finding not found.");

            await _repository.DeleteAsync(finding);
        }

        private static DoctorFindingResponseDto Map(DoctorFinding f)
        {
            return new DoctorFindingResponseDto
            {
                FindingId = f.FindingId,
                AppointmentId = f.AppointmentId,
                Observations = f.Observations,
                Recommendations = f.Recommendations,
                NextSessionDate = f.NextSessionDate,
                CreatedAt = f.CreatedAt
            };
        }

        private ClaimsPrincipal GetCurrentUserOrThrow()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Authentication required.");

            return user;
        }

        private async Task EnsureDoctorOwnsAppointmentForFindingWriteAsync(Appointment appointment)
        {
            var user = GetCurrentUserOrThrow();
            if (!user.IsInRole("Doctor"))
                throw new ForbiddenException("Only doctors can modify findings for appointments.");

            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorRepository.GetByUserIdAsync(userId);
            if (doctor == null || doctor.DoctorId != appointment.DoctorId)
                throw new ForbiddenException("You can only add or edit findings for your own appointments.");
        }

        private async Task EnsureCanViewFindingForAppointmentAsync(Appointment appointment)
        {
            var user = GetCurrentUserOrThrow();

            if (user.IsInRole("Admin") || user.IsInRole("Receptionist"))
                return;

            if (user.IsInRole("Doctor"))
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var doctor = await _doctorRepository.GetByUserIdAsync(userId);
                if (doctor == null || doctor.DoctorId != appointment.DoctorId)
                    throw new ForbiddenException("You do not have access to findings for this appointment.");

                return;
            }

            if (user.IsInRole("Guardian"))
            {
                var guardianId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (appointment.Patient?.GuardianId != guardianId)
                    throw new ForbiddenException("You do not have access to findings for this appointment.");

                return;
            }

            throw new ForbiddenException("You do not have access to findings for this appointment.");
        }
    }
}
