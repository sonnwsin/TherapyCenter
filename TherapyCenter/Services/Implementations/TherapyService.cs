using TherapyCenter.DTOs.Therapy;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class TherapyService : ITherapyService
    {
        private readonly ITherapyRepository _therapyRepository;

        public TherapyService(ITherapyRepository therapyRepository)
        {
            _therapyRepository = therapyRepository;
        }


        public async Task<TherapyResponseDto> CreateTherapyAsync(CreateTherapyDto dto)
        {
            var therapy = new Therapy
            {
                Name = dto.Name,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                Cost = dto.Cost
            };

            var created = await _therapyRepository.AddAsync(therapy);

            return MapToResponse(created);
        }



        public async Task<List<TherapyResponseDto>> GetAllTherapiesAsync()
        {
            var therapies = await _therapyRepository.GetAllAsync();

            return therapies.Select(MapToResponse).ToList();
        }



        public async Task<TherapyResponseDto> GetTherapyByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid therapy id.");

            var therapy = await _therapyRepository.GetByIdAsync(id);

            if (therapy == null)
                throw new Exception("Therapy not found");

            return MapToResponse(therapy);
        }



        public async Task<TherapyResponseDto> UpdateTherapyAsync(int id, UpdateTherapyDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid therapy id.");

            var therapy = await _therapyRepository.GetByIdAsync(id);

            if (therapy == null)
                throw new Exception("Therapy not found");

            therapy.Name = dto.Name;
            therapy.Description = dto.Description;
            therapy.DurationMinutes = dto.DurationMinutes;
            therapy.Cost = dto.Cost;

            await _therapyRepository.UpdateAsync(therapy);

            return MapToResponse(therapy);
        }


        public async Task DeleteTherapyAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid therapy id.");

            var therapy = await _therapyRepository.GetByIdAsync(id);

            if (therapy == null)
                throw new Exception("Therapy not found");

            await _therapyRepository.DeleteAsync(therapy);
        }



        private static TherapyResponseDto MapToResponse(Therapy therapy)
        {
            return new TherapyResponseDto
            {
                TherapyId = therapy.TherapyId,
                Name = therapy.Name,
                Description = therapy.Description,
                DurationMinutes = therapy.DurationMinutes,
                Cost = therapy.Cost
            };
        }
    }
}