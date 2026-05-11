using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IDoctorFindingRepository
    {
        Task<DoctorFinding> AddAsync(DoctorFinding finding);
        Task<List<DoctorFinding>> GetAllAsync();
        Task<DoctorFinding?> GetByIdAsync(int id);
        Task<List<DoctorFinding>> GetByAppointmentIdAsync(int appointmentId);
        Task UpdateAsync(DoctorFinding finding);
        Task DeleteAsync(DoctorFinding finding);
    }
}