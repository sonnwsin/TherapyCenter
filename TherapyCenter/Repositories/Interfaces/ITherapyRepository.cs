using TherapyCenter.DTOs.Therapy;
using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface ITherapyRepository
    {
        Task<Therapy> AddAsync(Therapy therapy);
        Task<List<Therapy>> GetAllAsync();
        Task<Therapy?> GetByIdAsync(int id);
        Task UpdateAsync(Therapy therapy);
        Task DeleteAsync(Therapy therapy);
        Task<List<TherapyPriceDto>> GetAllTherapyPricesAsync();
    }
}