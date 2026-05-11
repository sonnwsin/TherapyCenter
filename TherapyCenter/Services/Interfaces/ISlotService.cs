using TherapyCenter.DTOs.Slot;

namespace TherapyCenter.Services.Interfaces
{
    public interface ISlotService
    {
        Task<SlotResponseDto> CreateSlotAsync(CreateSlotDto dto);
        Task<List<SlotResponseDto>> GetAllSlotsAsync();
        Task<List<SlotResponseDto>> GetSlotsByDoctorAsync(int doctorId, DateOnly date);
        Task<SlotResponseDto> GetSlotByIdAsync(int id);
        Task<SlotResponseDto> UpdateSlotAsync(int id, UpdateSlotDto dto);
        Task DeleteSlotAsync(int id);
        /// <param name="therapyId">When set, slot length matches therapy duration; when null, legacy 30-minute grid.</param>
        Task<List<DoctorSlotDto>> GetGeneratedSlotsByDoctorAsync(int doctorId, DateOnly date, int? therapyId = null);
    }
}