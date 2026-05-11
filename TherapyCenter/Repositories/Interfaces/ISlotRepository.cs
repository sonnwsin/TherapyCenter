using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot> AddAsync(Slot slot);
        Task<List<Slot>> GetAllAsync();
        Task<List<Slot>> GetByDoctorAndDateAsync(int doctorId, DateOnly date);
        Task<Slot?> GetByIdAsync(int id);
        Task UpdateAsync(Slot slot);
        Task DeleteAsync(Slot slot);


    }
}