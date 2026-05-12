using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TherapyCenter.DTOs.DoctorFinding;
using TherapyCenter.Exceptions;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class DoctorFindingServiceTests
    {
        private readonly Mock<IDoctorFindingRepository> _findingRepositoryMock = new();
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly HttpContextAccessor _httpContextAccessor = new();
        private readonly DoctorFindingService _service;

        public DoctorFindingServiceTests()
        {
            _httpContextAccessor.HttpContext = new DefaultHttpContext
            {
                User = CreateUser(5, "Doctor")
            };

            _service = new DoctorFindingService(
                _findingRepositoryMock.Object,
                _appointmentRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _httpContextAccessor);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Finding_When_Doctor_Owns_Appointment()
        {
            // Arrange
            var dto = new CreateDoctorFindingDto
            {
                AppointmentId = 1,
                Observations = "Patient improved",
                Recommendations = "Continue therapy"
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.AppointmentId))
                .ReturnsAsync(new Appointment { AppointmentId = dto.AppointmentId, DoctorId = 10 });

            _doctorRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(5))
                .ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5 });

            _findingRepositoryMock
                .Setup(repo => repo.GetByAppointmentIdAsync(dto.AppointmentId))
                .ReturnsAsync(new List<DoctorFinding>());

            _findingRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<DoctorFinding>()))
                .ReturnsAsync((DoctorFinding finding) =>
                {
                    finding.FindingId = 7;
                    return finding;
                });

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.FindingId.Should().Be(7);
            result.AppointmentId.Should().Be(dto.AppointmentId);
            result.Observations.Should().Be(dto.Observations);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Doctor_Does_Not_Own_Appointment()
        {
            // Arrange
            var dto = new CreateDoctorFindingDto
            {
                AppointmentId = 1,
                Observations = "Notes"
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.AppointmentId))
                .ReturnsAsync(new Appointment { AppointmentId = dto.AppointmentId, DoctorId = 10 });

            _doctorRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(5))
                .ReturnsAsync(new Doctor { DoctorId = 99, UserId = 5 });

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You can only add or edit findings for your own appointments.");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Finding_Already_Exists_For_Appointment()
        {
            // Arrange
            var dto = new CreateDoctorFindingDto
            {
                AppointmentId = 1,
                Observations = "Duplicate"
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.AppointmentId))
                .ReturnsAsync(new Appointment { AppointmentId = dto.AppointmentId, DoctorId = 10 });

            _doctorRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(5))
                .ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5 });

            _findingRepositoryMock
                .Setup(repo => repo.GetByAppointmentIdAsync(dto.AppointmentId))
                .ReturnsAsync(new List<DoctorFinding>
                {
                    new() { FindingId = 3, AppointmentId = dto.AppointmentId }
                });

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("A finding already exists for this appointment.");
            _findingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<DoctorFinding>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_Should_Throw_When_User_Is_Not_Admin_Or_Receptionist()
        {
            // Arrange
            _httpContextAccessor.HttpContext!.User = CreateUser(5, "Doctor");

            // Act
            Func<Task> act = async () => await _service.GetAllAsync();

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to list all findings.");
        }

        private static ClaimsPrincipal CreateUser(int userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }
    }
}
