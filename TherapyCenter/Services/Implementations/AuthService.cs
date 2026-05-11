using TherapyCenter.DTOs.Auth;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;

        public AuthService(IUserRepository userRepository, IJwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("User already exists");

            var allowedRoles = new[] { "Receptionist", "Doctor", "Guardian" };

            if (!allowedRoles.Contains(request.Role))
                throw new Exception("Invalid role selection");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber
            };

            var createdUser = await _userRepository.AddUserAsync(user);
            var token = _jwtHelper.GenerateToken(createdUser);

            return new AuthResponseDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                Role = createdUser.Role,
                Token = token
            };
        }


        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                return null;

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return null;
            var token = _jwtHelper.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }
    }
}
