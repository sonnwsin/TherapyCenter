using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TherapyCenter.DTOs.Appointment;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    //[Route("api/appointment")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorRepository _doctorRepository;

        public AppointmentController(
            IAppointmentService appointmentService,
            IDoctorRepository doctorRepository)
        {
            _appointmentService = appointmentService;
            _doctorRepository = doctorRepository;
        }


        /// <summary>Legacy slot-based booking. Guardians book for their patients; receptionists book for any patient. (No Patient-user link in the model.)</summary>
        [Authorize(Roles = "Guardian,Receptionist")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            var result = await _appointmentService.CreateAsync(dto);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _appointmentService.GetAllAsync();
            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _appointmentService.GetByIdAsync(id);
            return Ok(result);
        }


        [Authorize(Roles = "Receptionist,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            var result = await _appointmentService.UpdateAsync(id, dto);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _appointmentService.DeleteAsync(id);
            return Ok(new { message = "Appointment deleted successfully" });
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            await _appointmentService.CompleteAsync(id);
            return Ok(new { message = "Appointment completed" });
        }

        [Authorize(Roles = "Receptionist,Admin,Doctor")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentService.CancelAsync(id);
            return Ok(new { message = "Appointment cancelled" });
        }



        [Authorize(Roles = "Receptionist")]
        [HttpPost("walkin")]
        public async Task<IActionResult> CreateWalkIn([FromBody] CreateWalkInAppointmentDto dto)
        {
            var result = await _appointmentService.CreateWalkInAsync(dto);
            return Ok(result);
        }


        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor")]
        public async Task<IActionResult> GetForDoctor()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _doctorRepository.GetByUserIdAsync(userId);

            if (doctor == null)
                return BadRequest(new { success = false, message = "Doctor not found.", userId });

            var result = await _appointmentService.GetByDoctorAsync(doctor.DoctorId);

            return Ok(new
            {
                userId,
                doctorId = doctor.DoctorId,
                data = result
            });
        }



        [Authorize(Roles = "Guardian")]
        [HttpPost("online")]
        public async Task<IActionResult> BookOnline([FromBody] BookOnlineAppointmentDto dto)
        {
            var result = await _appointmentService.BookOnlineAsync(dto);
            return Ok(result);
        }

        [Authorize(Roles = "Guardian")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var result = await _appointmentService.GetAppointmentsForGuardianAsync();
            return Ok(result);
        }

    }
}