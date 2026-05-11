using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.Therapy;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class TherapyServiceTests
    {
        private readonly Mock<ITherapyRepository> _therapyRepositoryMock;
        private readonly TherapyService _therapyService;

        public TherapyServiceTests()
        {
            _therapyRepositoryMock = new Mock<ITherapyRepository>();
            _therapyService = new TherapyService(_therapyRepositoryMock.Object);
        }

<<<<<<< HEAD
=======
        // ================= CREATE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task CreateTherapy_Should_Create_Successfully()
        {
            // Arrange
            var dto = new CreateTherapyDto
            {
                Name = "Speech Therapy",
                Description = "Speech improvement",
                DurationMinutes = 30,
                Cost = 500
            };

            var therapy = new Therapy
            {
                TherapyId = 1,
                Name = dto.Name,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                Cost = dto.Cost
            };

            _therapyRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Therapy>()))
                .ReturnsAsync(therapy);

            // Act
            var result = await _therapyService.CreateTherapyAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
        }

<<<<<<< HEAD
=======
        // ================= GET ALL =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task GetAllTherapies_Should_Return_List()
        {
            // Arrange
            var therapies = new List<Therapy>
            {
                new Therapy { TherapyId = 1, Name = "A", Cost = 100 },
                new Therapy { TherapyId = 2, Name = "B", Cost = 200 }
            };

            _therapyRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(therapies);

            // Act
            var result = await _therapyService.GetAllTherapiesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

<<<<<<< HEAD
=======
        // ================= GET BY ID =================

>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e
        [Fact]
        public async Task GetTherapyById_Should_Return_Therapy_When_Exists()
        {
            // Arrange
            var therapy = new Therapy
            {
                TherapyId = 1,
                Name = "Speech Therapy"
            };

            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(therapy);

            // Act
            var result = await _therapyService.GetTherapyByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TherapyId.Should().Be(1);
        }

        [Fact]
        public async Task GetTherapyById_Should_Throw_Exception_When_Not_Found()
        {
            // Arrange
            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Therapy?)null);

            // Act
            Func<Task> act = async () => await _therapyService.GetTherapyByIdAsync(1);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Therapy not found");
        }

<<<<<<< HEAD
=======
        // ================= UPDATE =================

>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e
        [Fact]
        public async Task UpdateTherapy_Should_Update_When_Exists()
        {
            // Arrange
            var existing = new Therapy
            {
                TherapyId = 1,
                Name = "Old"
            };

            var dto = new UpdateTherapyDto
            {
                Name = "New",
                Description = "Updated",
                DurationMinutes = 60,
                Cost = 1000
            };

            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _therapyRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Therapy>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _therapyService.UpdateTherapyAsync(1, dto);

            // Assert
            result.Name.Should().Be("New");
        }

        [Fact]
        public async Task UpdateTherapy_Should_Throw_Exception_When_Not_Found()
        {
            // Arrange
            var dto = new UpdateTherapyDto
            {
                Name = "New"
            };

            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Therapy?)null);

            // Act
            Func<Task> act = async () => await _therapyService.UpdateTherapyAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Therapy not found");
        }

<<<<<<< HEAD
=======
        // ================= DELETE =================

>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e
        [Fact]
        public async Task DeleteTherapy_Should_Delete_When_Exists()
        {
            // Arrange
            var therapy = new Therapy { TherapyId = 1 };

            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(therapy);

            _therapyRepositoryMock
                .Setup(x => x.DeleteAsync(therapy))
                .Returns(Task.CompletedTask);

            // Act
            await _therapyService.DeleteTherapyAsync(1);

            // Assert
            _therapyRepositoryMock.Verify(x => x.DeleteAsync(therapy), Times.Once);
        }

        [Fact]
        public async Task DeleteTherapy_Should_Throw_Exception_When_Not_Found()
        {
            // Arrange
            _therapyRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Therapy?)null);

            // Act
            Func<Task> act = async () => await _therapyService.DeleteTherapyAsync(1);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Therapy not found");
        }
    }
}