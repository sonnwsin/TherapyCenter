using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TherapyCenter.DTOs.Auth;
using TherapyCenter.Helpers;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IJwtHelper> _jwtHelperMock = new();
        private readonly Mock<IDistributedCache> _cacheMock = new();
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _cacheMock
                .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                .Returns(Task.CompletedTask);
            _cacheMock
                .Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);

            _service = new AuthService(
                _userRepositoryMock.Object,
                _jwtHelperMock.Object,
                _cacheMock.Object,
                _emailMock.Object,
                NullLogger<AuthService>.Instance);
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Response_When_Credentials_Are_Valid()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "doctor@test.com",
                Password = "password123"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Doctor",
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Doctor"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtHelperMock
                .Setup(jwt => jwt.GenerateToken(user))
                .Returns("test-token");

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(request.Email);
            result.Role.Should().Be("Doctor");
            result.Token.Should().Be("test-token");
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Null_When_Password_Is_Invalid()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "doctor@test.com",
                Password = "wrong-password"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "Test",
                LastName = "Doctor",
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
                Role = "Doctor"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            result.Should().BeNull();
            _jwtHelperMock.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_When_Email_Already_Exists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@test.com",
                Password = "password123",
                Role = "Guardian"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });

            // Act
            Func<Task> act = async () => await _service.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("User already exists");
        }

        [Fact]
        public async Task RegisterAsync_Should_Generate_Jwt_For_New_User()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                FirstName = "Jane",
                LastName = "Guardian",
                Email = "jane@test.com",
                Password = "password123",
                Role = "Guardian",
                PhoneNumber = "9999999999"
            };

            var createdUser = new User
            {
                UserId = 10,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _jwtHelperMock
                .Setup(jwt => jwt.GenerateToken(createdUser))
                .Returns("new-user-token");

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            result.UserId.Should().Be(createdUser.UserId);
            result.Email.Should().Be(request.Email);
            result.Token.Should().Be("new-user-token");
            _jwtHelperMock.Verify(jwt => jwt.GenerateToken(createdUser), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordForGuardianAsync_Should_Throw_When_User_Is_Not_Guardian()
        {
            var dto = new ForgotPasswordRequestDto { Email = "doc@test.com" };
            _userRepositoryMock
                .Setup(r => r.GetByEmailIgnoreCaseAsync(dto.Email))
                .ReturnsAsync(new User
                {
                    Email = dto.Email,
                    Role = "Doctor",
                    PasswordHash = "x"
                });

            Func<Task> act = async () => await _service.ForgotPasswordForGuardianAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("No guardian account found for this email.");
            _emailMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
