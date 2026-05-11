using TherapyCenter.DTOs.Patient;

namespace TherapyCenter.Services.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientSummaryDto>> GetMyPatientsForGuardianAsync();
    }
}
