using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TherapyCenter.DTOs.Therapy;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class TherapyService : ITherapyService
    {
        /// <summary>Redis entry key for the therapy price list (cache-aside).</summary>
        private const string TherapyPricesCacheKey = "therapy_prices";

        private readonly ITherapyRepository _therapyRepository;
        private readonly IDistributedCache _cache;

        public TherapyService(ITherapyRepository therapyRepository, IDistributedCache cache)
        {
            _therapyRepository = therapyRepository;
            _cache = cache;
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

            // Cache invalidation: the stored list of all prices is now stale, so remove it from Redis.
            await _cache.RemoveAsync(TherapyPricesCacheKey);

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

        /// <summary>
        /// Returns all therapy prices using cache-aside against Redis (<see cref="IDistributedCache"/>).
        /// </summary>
        public async Task<List<TherapyPriceDto>> GetAllTherapyPricesAsync()
        {
            // --- Step 1: Try Redis first (fast path) ---
            // GetAsync returns null if the key does not exist or Redis is empty for that key.
            var cachedBytes = await _cache.GetAsync(TherapyPricesCacheKey);

            if (cachedBytes is { Length: > 0 })
            {
                // --- Step 2: Deserialize JSON bytes produced when we populated the cache ---
                var fromCache = JsonSerializer.Deserialize<List<TherapyPriceDto>>(cachedBytes);
                if (fromCache is not null)
                    return fromCache;
                // If deserialization fails, fall through and rebuild from SQL (defensive).
            }

            // --- Step 3: Cache miss (or bad payload) — load from SQL via repository projection ---
            var fromDb = await _therapyRepository.GetAllTherapyPricesAsync();

            // --- Step 4: Serialize to UTF-8 JSON bytes (same shape we deserialize in step 2) ---
            var payload = JsonSerializer.SerializeToUtf8Bytes(fromDb);

            // --- Step 5: Write to Redis with a TTL so prices refresh periodically even without updates ---
            await _cache.SetAsync(
                TherapyPricesCacheKey,
                payload,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });

            // --- Step 6: Return fresh data to the caller (and callers after this hit Redis until expiry or invalidation) ---
            return fromDb;
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
