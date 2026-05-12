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
        private readonly Mock<ITherapyRepository> _therapyRepositoryMock = new();
        private readonly TherapyService _service;

        public TherapyServiceTests()
        {
            _service = new TherapyService(_therapyRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateTherapyAsync_Should_Create_Therapy()
        {
            // Arrange
            var dto = new CreateTherapyDto
            {
                Name = "Speech Therapy",
                Description = "Speech improvement",
                DurationMinutes = 30,
                Cost = 500
            };

            _therapyRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Therapy>()))
                .ReturnsAsync((Therapy therapy) =>
                {
                    therapy.TherapyId = 1;
                    return therapy;
                });

            // Act
            var result = await _service.CreateTherapyAsync(dto);

            // Assert
            result.TherapyId.Should().Be(1);
            result.Name.Should().Be(dto.Name);
            result.DurationMinutes.Should().Be(dto.DurationMinutes);
        }

        [Fact]
        public async Task GetTherapyByIdAsync_Should_Return_Therapy_When_Found()
        {
            // Arrange
            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Therapy
                {
                    TherapyId = 1,
                    Name = "Occupational Therapy",
                    DurationMinutes = 45,
                    Cost = 700
                });

            // Act
            var result = await _service.GetTherapyByIdAsync(1);

            // Assert
            result.TherapyId.Should().Be(1);
            result.Name.Should().Be("Occupational Therapy");
        }

        [Fact]
        public async Task GetTherapyByIdAsync_Should_Throw_When_Not_Found()
        {
            // Arrange
            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((Therapy?)null);

            // Act
            Func<Task> act = async () => await _service.GetTherapyByIdAsync(1);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Therapy not found");
        }

        [Fact]
        public async Task UpdateTherapyAsync_Should_Update_Therapy()
        {
            // Arrange
            var therapy = new Therapy
            {
                TherapyId = 1,
                Name = "Old Name",
                DurationMinutes = 30,
                Cost = 500
            };

            var dto = new UpdateTherapyDto
            {
                Name = "New Name",
                Description = "Updated",
                DurationMinutes = 60,
                Cost = 1000
            };

            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(therapy);

            // Act
            var result = await _service.UpdateTherapyAsync(1, dto);

            // Assert
            result.Name.Should().Be(dto.Name);
            result.DurationMinutes.Should().Be(dto.DurationMinutes);
            _therapyRepositoryMock.Verify(repo => repo.UpdateAsync(therapy), Times.Once);
        }

        [Fact]
        public async Task DeleteTherapyAsync_Should_Delete_When_Found()
        {
            // Arrange
            var therapy = new Therapy { TherapyId = 1, Name = "Therapy", DurationMinutes = 30, Cost = 500 };

            _therapyRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(therapy);

            // Act
            await _service.DeleteTherapyAsync(1);

            // Assert
            _therapyRepositoryMock.Verify(repo => repo.DeleteAsync(therapy), Times.Once);
        }
    }
}
