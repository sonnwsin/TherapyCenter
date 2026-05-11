using TherapyCenter.DTOs.User;

namespace TherapyCenter.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserResponseDto>> ListAllUsersAsync();
        Task<List<UserResponseDto>> ListUsersByRoleAsync(string role);
        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto dto);
        Task DeleteUserAsync(int id);
    }
}
