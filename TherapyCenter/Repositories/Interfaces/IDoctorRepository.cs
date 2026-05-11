using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor> AddAsync(Doctor doctor);
        Task<List<Doctor>> GetAllAsync();
        Task<Doctor?> GetByIdAsync(int id);
        Task UpdateAsync(Doctor doctor);
        Task DeleteAsync(Doctor doctor);


        Task<Doctor?> GetByUserIdAsync(int userId);


        Task<List<Doctor>> GetAllWithUserAsync();
    }
}