using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient> AddAsync(Patient patient);
        Task<Patient?> GetByIdAsync(int id);
        Task<List<Patient>> GetAllAsync();

        Task<Patient?> FindByNameAsync(string firstName, string lastName);

        Task<List<Patient>> GetByGuardianIdAsync(int guardianId);
    }
}