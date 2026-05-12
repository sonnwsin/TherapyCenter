using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.Slot;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class SlotServiceTests
    {
        private readonly Mock<ISlotRepository> _slotRepositoryMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
        private readonly Mock<ITherapyRepository> _therapyRepositoryMock = new();
        private readonly SlotService _service;

        public SlotServiceTests()
        {
            _service = new SlotService(
                _slotRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _appointmentRepositoryMock.Object,
                _therapyRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateSlotAsync_Should_Create_Slot_When_Time_Is_Valid()
        {
            // Arrange
            var dto = new CreateSlotDto
            {
                DoctorId = 1,
                Date = new DateOnly(2026, 5, 12),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(10, 30)
            };

            _slotRepositoryMock
                .Setup(repo => repo.GetByDoctorAndDateAsync(dto.DoctorId, dto.Date))
                .ReturnsAsync(new List<Slot>());

            _slotRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Slot>()))
                .ReturnsAsync((Slot slot) =>
                {
                    slot.SlotId = 1;
                    return slot;
                });

            // Act
            var result = await _service.CreateSlotAsync(dto);

            // Assert
            result.SlotId.Should().Be(1);
            result.DoctorId.Should().Be(dto.DoctorId);
            result.IsBooked.Should().BeFalse();
        }

        [Fact]
        public async Task CreateSlotAsync_Should_Throw_When_Time_Range_Is_Invalid()
        {
            // Arrange
            var dto = new CreateSlotDto
            {
                DoctorId = 1,
                Date = new DateOnly(2026, 5, 12),
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(10, 0)
            };

            // Act
            Func<Task> act = async () => await _service.CreateSlotAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid time range");
        }

        [Fact]
        public async Task CreateSlotAsync_Should_Throw_When_Slot_Overlaps()
        {
            // Arrange
            var dto = new CreateSlotDto
            {
                DoctorId = 1,
                Date = new DateOnly(2026, 5, 12),
                StartTime = new TimeOnly(10, 15),
                EndTime = new TimeOnly(10, 45)
            };

            _slotRepositoryMock
                .Setup(repo => repo.GetByDoctorAndDateAsync(dto.DoctorId, dto.Date))
                .ReturnsAsync(new List<Slot>
                {
                    new()
                    {
                        DoctorId = dto.DoctorId,
                        Date = dto.Date,
                        StartTime = new TimeOnly(10, 0),
                        EndTime = new TimeOnly(10, 30)
                    }
                });

            // Act
            Func<Task> act = async () => await _service.CreateSlotAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot overlaps with existing slot");
        }

        [Fact]
        public async Task GetGeneratedSlotsByDoctorAsync_Should_Mark_Overlapping_Appointment_As_Booked()
        {
            // Arrange
            var date = new DateOnly(2026, 5, 12);

            _doctorRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Doctor
                {
                    DoctorId = 1,
                    AvailableDays = "Mon-Fri",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0)
                });

            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(2))
                .ReturnsAsync(new Therapy
                {
                    TherapyId = 2,
                    Name = "Speech Therapy",
                    DurationMinutes = 30,
                    Cost = 500
                });

            _appointmentRepositoryMock
                .Setup(repo => repo.GetByDoctorAndDateAsync(1, date))
                .ReturnsAsync(new List<Appointment>
                {
                    new()
                    {
                        DoctorId = 1,
                        AppointmentDate = date,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(9, 30),
                        Status = "Scheduled"
                    }
                });

            // Act
            var result = await _service.GetGeneratedSlotsByDoctorAsync(1, date, therapyId: 2);

            // Assert
            result.Should().HaveCount(2);
            result[0].Status.Should().Be("Booked");
            result[1].Status.Should().Be("Available");
        }

        [Fact]
        public async Task DeleteSlotAsync_Should_Delete_When_Found()
        {
            // Arrange
            var slot = new Slot { SlotId = 1 };

            _slotRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(slot);

            // Act
            await _service.DeleteSlotAsync(1);

            // Assert
            _slotRepositoryMock.Verify(repo => repo.DeleteAsync(slot), Times.Once);
        }
    }
}
