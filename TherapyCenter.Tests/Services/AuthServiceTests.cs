using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.Auth;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtHelper> _jwtHelperMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtHelperMock = new Mock<IJwtHelper>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _jwtHelperMock.Object
            );
        }

<<<<<<< HEAD
=======
        // ================= LOGIN TESTS =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "123456"
            };

            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Patient"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _jwtHelperMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be("fake-jwt-token");
        }

        [Fact]
        public async Task Login_Should_Return_Null_When_User_Not_Found()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "notfound@example.com",
                Password = "123456"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_Should_Return_Null_When_Password_Is_Invalid()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Patient"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        // ================= REGISTER TESTS =================

        [Fact]
        public async Task Register_Should_Create_User_When_Data_Is_Valid()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "123456",
                Role = "Doctor",
                PhoneNumber = "1234567890"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.Role.Should().Be(request.Role);
        }

        [Fact]
        public async Task Register_Should_Throw_Exception_When_User_Already_Exists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "123456",
                Role = "Doctor"
            };

            var existingUser = new User { Email = request.Email };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("User already exists");
        }

        [Fact]
        public async Task Register_Should_Throw_Exception_When_Role_Is_Invalid()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "123456",
                Role = "Admin" // ❌ not allowed
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid role selection");
        }
    }
}