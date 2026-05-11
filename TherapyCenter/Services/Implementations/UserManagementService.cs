using System.Security.Claims;
using TherapyCenter.DTOs.User;
using TherapyCenter.Exceptions;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<UserResponseDto>> ListAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserResponse).ToList();
        }

        public async Task<List<UserResponseDto>> ListUsersByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role is required.");

            var users = await _userRepository.GetByRoleAsync(role.Trim());
            return users.Select(MapToUserResponse).ToList();
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user id.");

            if (dto == null)
                throw new ArgumentException("Request body is required.");

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                throw new Exception("First name is required.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                throw new Exception("Last name is required.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new Exception("Email is required.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found.");

            var emailToUse = dto.Email.Trim();
            if (!string.Equals(user.Email, emailToUse, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByEmailAsync(emailToUse);
                if (existing != null && existing.UserId != id)
                    throw new Exception("Email is already in use by another account.");
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = emailToUse;
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepository.UpdateAsync(user);

            return MapToUserResponse(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user id.");

            var currentId = GetCurrentUserIdOrThrow();
            if (id == currentId)
                throw new ForbiddenException("You cannot delete your own account.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found.");

            await _userRepository.DeleteAsync(user);
        }

        private int GetCurrentUserIdOrThrow()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Authentication required.");

            return int.Parse(claim.Value);
        }

        private static UserResponseDto MapToUserResponse(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
