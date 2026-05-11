using TherapyCenter.DTOs.Therapy;

namespace TherapyCenter.Services.Interfaces
{
    public interface ITherapyService
    {
        Task<TherapyResponseDto> CreateTherapyAsync(CreateTherapyDto dto);
        Task<List<TherapyResponseDto>> GetAllTherapiesAsync();
        Task<TherapyResponseDto> GetTherapyByIdAsync(int id);
        Task<TherapyResponseDto> UpdateTherapyAsync(int id, UpdateTherapyDto dto);
        Task DeleteTherapyAsync(int id);
    }
}