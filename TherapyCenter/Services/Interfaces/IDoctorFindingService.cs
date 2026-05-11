using TherapyCenter.DTOs.DoctorFinding;

namespace TherapyCenter.Services.Interfaces
{
    public interface IDoctorFindingService
    {
        Task<DoctorFindingResponseDto> CreateAsync(CreateDoctorFindingDto dto);
        Task<List<DoctorFindingResponseDto>> GetAllAsync();
        Task<DoctorFindingResponseDto> GetByIdAsync(int id);
        Task<List<DoctorFindingResponseDto>> GetByAppointmentAsync(int appointmentId);
        Task<DoctorFindingResponseDto> UpdateAsync(int id, UpdateDoctorFindingDto dto);
        Task DeleteAsync(int id);
    }
}