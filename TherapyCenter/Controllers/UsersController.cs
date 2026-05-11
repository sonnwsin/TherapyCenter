using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTOs.User;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManagementService.ListAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("receptionists")]
        public async Task<IActionResult> GetReceptionists()
        {
            var users = await _userManagementService.ListUsersByRoleAsync("Receptionist");
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            var updated = await _userManagementService.UpdateUserAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userManagementService.DeleteUserAsync(id);
            return Ok(new { success = true, message = "User deleted successfully" });
        }
    }
}
