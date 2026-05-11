using System.Security.Claims;
using TherapyCenter.DTOs.Patient;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatientService(IPatientRepository patientRepository, IHttpContextAccessor httpContextAccessor)
        {
            _patientRepository = patientRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<PatientSummaryDto>> GetMyPatientsForGuardianAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Authentication required.");

            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new Exception("User id not found in token.");

            var guardianId = int.Parse(claim.Value);
            var patients = await _patientRepository.GetByGuardianIdAsync(guardianId);

            return patients
                .Select(p => new PatientSummaryDto
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    MedicalHistory = p.MedicalHistory,
                    CreatedAt = p.CreatedAt
                })
                .ToList();
        }
    }
}
