using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>Four totals: users, appointments, doctors, therapies (admin only).</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> Summary(CancellationToken cancellationToken = default)
        {
            var result = await _reportService.GetSummaryAsync(cancellationToken);
            return Ok(result);
        }
    }
}
