using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Guardian")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>Patients linked to the current guardian account.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _patientService.GetMyPatientsForGuardianAsync();
            return Ok(result);
        }
    }
}
