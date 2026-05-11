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
        private readonly Mock<ISlotRepository> _slotRepoMock;
        private readonly SlotService _service;

        public SlotServiceTests()
        {
            _slotRepoMock = new Mock<ISlotRepository>();
            _service = new SlotService(_slotRepoMock.Object);
        }

<<<<<<< HEAD
=======
        // ================= CREATE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task CreateSlot_Should_Create_When_Valid()
        {
            // Arrange
            var dto = new CreateSlotDto
            {
                DoctorId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0)
            };

            _slotRepoMock
                .Setup(x => x.GetByDoctorAndDateAsync(dto.DoctorId, dto.Date))
                .ReturnsAsync(new List<Slot>());

            _slotRepoMock
                .Setup(x => x.AddAsync(It.IsAny<Slot>()))
                .ReturnsAsync((Slot s) => s);

            // Act
            var result = await _service.CreateSlotAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.DoctorId.Should().Be(dto.DoctorId);
        }

        [Fact]
        public async Task CreateSlot_Should_Throw_When_Time_Invalid()
        {
            var dto = new CreateSlotDto
            {
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(10, 0)
            };

            Func<Task> act = async () => await _service.CreateSlotAsync(dto);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid time range");
        }

        [Fact]
        public async Task CreateSlot_Should_Throw_When_Overlap()
        {
            var dto = new CreateSlotDto
            {
                DoctorId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(10, 30),
                EndTime = new TimeOnly(11, 30)
            };

            var existing = new List<Slot>
            {
                new Slot
                {
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(11, 0)
                }
            };

            _slotRepoMock
                .Setup(x => x.GetByDoctorAndDateAsync(dto.DoctorId, dto.Date))
                .ReturnsAsync(existing);

            Func<Task> act = async () => await _service.CreateSlotAsync(dto);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot overlaps with existing slot");
        }

<<<<<<< HEAD
=======
        // ================= GET =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task GetAllSlots_Should_Return_List()
        {
            _slotRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Slot> { new Slot(), new Slot() });

            var result = await _service.GetAllSlotsAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSlotsByDoctor_Should_Return_List()
        {
            _slotRepoMock
                .Setup(x => x.GetByDoctorAndDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Slot> { new Slot() });

            var result = await _service.GetSlotsByDoctorAsync(1, DateOnly.FromDateTime(DateTime.Today));

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSlotById_Should_Return_When_Exists()
        {
            _slotRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Slot { SlotId = 1 });

            var result = await _service.GetSlotByIdAsync(1);

            result.SlotId.Should().Be(1);
        }

        [Fact]
        public async Task GetSlotById_Should_Throw_When_Not_Found()
        {
            _slotRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Slot?)null);

            Func<Task> act = async () => await _service.GetSlotByIdAsync(1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot not found");
        }

<<<<<<< HEAD
=======
        // ================= UPDATE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task UpdateSlot_Should_Update_When_Valid()
        {
            var slot = new Slot
            {
                SlotId = 1,
                DoctorId = 1
            };

            var dto = new UpdateSlotDto
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            };

            _slotRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
            _slotRepoMock.Setup(x => x.GetByDoctorAndDateAsync(slot.DoctorId, dto.Date))
                .ReturnsAsync(new List<Slot>());
            _slotRepoMock.Setup(x => x.UpdateAsync(slot))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateSlotAsync(1, dto);

            result.StartTime.Should().Be(dto.StartTime);
        }

        [Fact]
        public async Task UpdateSlot_Should_Throw_When_Not_Found()
        {
            _slotRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Slot?)null);

            var dto = new UpdateSlotDto();

            Func<Task> act = async () => await _service.UpdateSlotAsync(1, dto);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot not found");
        }

<<<<<<< HEAD
=======
        // ================= DELETE =================

>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e
        [Fact]
        public async Task DeleteSlot_Should_Delete_When_Exists()
        {
            var slot = new Slot { SlotId = 1 };

            _slotRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(slot);

            _slotRepoMock.Setup(x => x.DeleteAsync(slot))
                .Returns(Task.CompletedTask);

            await _service.DeleteSlotAsync(1);

            _slotRepoMock.Verify(x => x.DeleteAsync(slot), Times.Once);
        }

        [Fact]
        public async Task DeleteSlot_Should_Throw_When_Not_Found()
        {
            _slotRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Slot?)null);

            Func<Task> act = async () => await _service.DeleteSlotAsync(1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Slot not found");
        }
    }
}