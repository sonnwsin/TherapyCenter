using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTOs.Therapy;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TherapyController : ControllerBase
    {
        private readonly ITherapyService _therapyService;

        public TherapyController(ITherapyService therapyService)
        {
            _therapyService = therapyService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTherapyDto dto)
        {
            var result = await _therapyService.CreateTherapyAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _therapyService.GetAllTherapiesAsync();
            return Ok(result);
        }

        /// <summary>All therapy prices; list is cached in Redis from TherapyService (cache-aside).</summary>
        [HttpGet("/api/therapy/prices")]
        public async Task<IActionResult> GetAllTherapyPrices()
        {
            var result = await _therapyService.GetAllTherapyPricesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _therapyService.GetTherapyByIdAsync(id);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTherapyDto dto)
        {
            var result = await _therapyService.UpdateTherapyAsync(id, dto);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _therapyService.DeleteTherapyAsync(id);
            return Ok(new { message = "Therapy deleted successfully" });
        }
    }
}