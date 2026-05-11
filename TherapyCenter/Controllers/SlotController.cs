using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTOs.Slot;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SlotController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotController(ISlotService slotService)
        {
            _slotService = slotService;
        }


        [Authorize(Roles = "Receptionist")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlotDto dto)
        {
            var result = await _slotService.CreateSlotAsync(dto);
            return Ok(result);
        }


        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _slotService.GetAllSlotsAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _slotService.GetSlotByIdAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] DateOnly date)
        {
            var result = await _slotService.GetSlotsByDoctorAsync(doctorId, date);
            return Ok(result);
        }



        [Authorize(Roles = "Receptionist")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSlotDto dto)
        {
            var result = await _slotService.UpdateSlotAsync(id, dto);
            return Ok(result);
        }


        [Authorize(Roles = "Receptionist")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _slotService.DeleteSlotAsync(id);
            return Ok(new { message = "Slot deleted successfully" });
        }



        [HttpGet("doctor/{doctorId}/generated")]
        public async Task<IActionResult> GetGenerated(int doctorId, [FromQuery] DateOnly date, [FromQuery] int? therapyId = null)
        {
            var result = await _slotService.GetGeneratedSlotsByDoctorAsync(doctorId, date, therapyId);
            return Ok(result);
        }
    }
}