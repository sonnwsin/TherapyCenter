using FluentAssertions;
using Moq;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class BookingValidationServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<ITherapyRepository> _therapyRepositoryMock = new();
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
        private readonly BookingValidationService _service;

        public BookingValidationServiceTests()
        {
            _service = new BookingValidationService(
                _doctorRepositoryMock.Object,
                _therapyRepositoryMock.Object,
                _appointmentRepositoryMock.Object);
        }

        [Fact]
        public async Task ValidateBookableWindowAsync_Should_Pass_When_Slot_Is_Valid()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12); // Tuesday
            SetupDoctor(date);
            SetupTherapy(durationMinutes: 30);
            SetupAppointments(new List<Appointment>());

            // Act
            Func<Task> act = async () => await _service.ValidateBookableWindowAsync(
                doctorId: 1,
                therapyId: 2,
                date,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ValidateBookableWindowAsync_Should_Throw_When_Doctor_Is_Not_Available_On_Date()
        {
            // Arrange
            var sunday = new DateOnly(2026, 5, 17);
            SetupDoctor(sunday);

            // Act
            Func<Task> act = async () => await _service.ValidateBookableWindowAsync(
                doctorId: 1,
                therapyId: 2,
                sunday,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Doctor is not available on the selected date.");
        }

        [Fact]
        public async Task ValidateBookableWindowAsync_Should_Throw_When_Therapy_Duration_Does_Not_Match()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);
            SetupDoctor(date);
            SetupTherapy(durationMinutes: 60);

            // Act
            Func<Task> act = async () => await _service.ValidateBookableWindowAsync(
                doctorId: 1,
                therapyId: 2,
                date,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Appointment length must be exactly 60 minutes for the selected therapy.");
        }

        [Fact]
        public async Task ValidateBookableWindowAsync_Should_Throw_When_Appointment_Overlaps()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);
            SetupDoctor(date);
            SetupTherapy(durationMinutes: 30);
            SetupAppointments(new List<Appointment>
            {
                new()
                {
                    DoctorId = 1,
                    AppointmentDate = date,
                    StartTime = new TimeOnly(10, 15),
                    EndTime = new TimeOnly(10, 45),
                    Status = "Scheduled"
                }
            });

            // Act
            Func<Task> act = async () => await _service.ValidateBookableWindowAsync(
                doctorId: 1,
                therapyId: 2,
                date,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("This time overlaps an existing appointment.");
        }

        [Fact]
        public async Task ValidateBookableWindowAsync_Should_Throw_When_Time_Range_Is_Invalid()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);
            SetupDoctor(date);
            SetupTherapy(durationMinutes: 30);

            // Act
            Func<Task> act = async () => await _service.ValidateBookableWindowAsync(
                doctorId: 1,
                therapyId: 2,
                date,
                new TimeOnly(11, 0),
                new TimeOnly(10, 30));

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid time range.");
        }

        private void SetupDoctor(DateOnly date)
        {
            _doctorRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Doctor
                {
                    DoctorId = 1,
                    AvailableDays = "Mon-Fri",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0)
                });
        }

        private void SetupTherapy(int durationMinutes)
        {
            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(2))
                .ReturnsAsync(new Therapy
                {
                    TherapyId = 2,
                    Name = "Speech Therapy",
                    DurationMinutes = durationMinutes,
                    Cost = 500
                });
        }

        private void SetupAppointments(List<Appointment> appointments)
        {
            _appointmentRepositoryMock
                .Setup(repo => repo.GetByDoctorAndDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(appointments);
        }
    }
}
