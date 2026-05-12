using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TherapyCenter.DTOs.Appointment;
using TherapyCenter.Exceptions;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
        private readonly Mock<ISlotRepository> _slotRepositoryMock = new();
        private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IBookingValidationService> _bookingValidationMock = new();
        private readonly HttpContextAccessor _httpContextAccessor = new();
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _httpContextAccessor.HttpContext = new DefaultHttpContext
            {
                User = CreateUser(5, "Receptionist")
            };

            _service = new AppointmentService(
                _appointmentRepositoryMock.Object,
                _slotRepositoryMock.Object,
                _patientRepositoryMock.Object,
                _httpContextAccessor,
                _doctorRepositoryMock.Object,
                _bookingValidationMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Appointment_When_Slot_Is_Available()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);
            var start = new TimeOnly(10, 0);
            var end = new TimeOnly(10, 30);
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 2,
                TherapyId = 3,
                SlotId = 4,
                Notes = "First visit"
            };

            var slot = new Slot
            {
                SlotId = dto.SlotId,
                DoctorId = dto.DoctorId,
                Date = date,
                StartTime = start,
                EndTime = end,
                IsBooked = false
            };

            _slotRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.SlotId))
                .ReturnsAsync(slot);

            _appointmentRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Appointment>()))
                .ReturnsAsync((Appointment appointment) =>
                {
                    appointment.AppointmentId = 20;
                    return appointment;
                });

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.AppointmentId.Should().Be(20);
            result.Status.Should().Be("Scheduled");
            result.StartTime.Should().Be(start);
            slot.IsBooked.Should().BeTrue();
            _slotRepositoryMock.Verify(repo => repo.UpdateAsync(slot), Times.Once);
            _bookingValidationMock.Verify(service => service.ValidateBookableWindowAsync(
                dto.DoctorId,
                dto.TherapyId,
                date,
                start,
                end), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Slot_Is_Already_Booked()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 2,
                TherapyId = 3,
                SlotId = 4
            };

            _slotRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.SlotId))
                .ReturnsAsync(new Slot
                {
                    SlotId = dto.SlotId,
                    DoctorId = dto.DoctorId,
                    IsBooked = true
                });

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot already booked");
        }

        [Fact]
        public async Task CreateAsync_Should_Stop_When_Booking_Validation_Fails()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 2,
                TherapyId = 3,
                SlotId = 4
            };

            var slot = new Slot
            {
                SlotId = dto.SlotId,
                DoctorId = dto.DoctorId,
                Date = date,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(10, 30)
            };

            _slotRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.SlotId))
                .ReturnsAsync(slot);

            _bookingValidationMock
                .Setup(service => service.ValidateBookableWindowAsync(
                    dto.DoctorId,
                    dto.TherapyId,
                    slot.Date,
                    slot.StartTime,
                    slot.EndTime))
                .ThrowsAsync(new Exception("This time overlaps an existing appointment."));

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("This time overlaps an existing appointment.");
            _appointmentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_Should_Set_Status_Cancelled_For_Receptionist()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentId = 1,
                Status = "Scheduled"
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(appointment.AppointmentId))
                .ReturnsAsync(appointment);

            // Act
            await _service.CancelAsync(appointment.AppointmentId);

            // Assert
            appointment.Status.Should().Be("Cancelled");
            _appointmentRepositoryMock.Verify(repo => repo.UpdateAsync(appointment), Times.Once);
        }

        [Fact]
        public async Task CompleteAsync_Should_Set_Status_Completed_When_Doctor_Owns_Appointment()
        {
            // Arrange
            _httpContextAccessor.HttpContext!.User = CreateUser(9, "Doctor");

            var appointment = new Appointment
            {
                AppointmentId = 1,
                DoctorId = 7,
                Status = "Scheduled"
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(appointment.AppointmentId))
                .ReturnsAsync(appointment);

            _doctorRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(9))
                .ReturnsAsync(new Doctor { DoctorId = appointment.DoctorId, UserId = 9 });

            // Act
            await _service.CompleteAsync(appointment.AppointmentId);

            // Assert
            appointment.Status.Should().Be("Completed");
            _appointmentRepositoryMock.Verify(repo => repo.UpdateAsync(appointment), Times.Once);
        }

        [Fact]
        public async Task CompleteAsync_Should_Throw_When_Doctor_Does_Not_Own_Appointment()
        {
            // Arrange
            _httpContextAccessor.HttpContext!.User = CreateUser(9, "Doctor");

            var appointment = new Appointment
            {
                AppointmentId = 1,
                DoctorId = 7
            };

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(appointment.AppointmentId))
                .ReturnsAsync(appointment);

            _doctorRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(9))
                .ReturnsAsync(new Doctor { DoctorId = 99, UserId = 9 });

            // Act
            Func<Task> act = async () => await _service.CompleteAsync(appointment.AppointmentId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You can only complete appointments assigned to you.");
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
