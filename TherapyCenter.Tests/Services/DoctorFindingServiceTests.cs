using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.DoctorFinding;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class DoctorFindingServiceTests
    {
        private readonly Mock<IDoctorFindingRepository> _repoMock;
        private readonly DoctorFindingService _service;

        public DoctorFindingServiceTests()
        {
            _repoMock = new Mock<IDoctorFindingRepository>();
            _service = new DoctorFindingService(_repoMock.Object);
        }

<<<<<<< HEAD

=======
        // ================= CREATE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task Create_Should_Create_Finding()
        {
            // Arrange
            var dto = new CreateDoctorFindingDto
            {
                AppointmentId = 1,
                Observations = "Obs",
                Recommendations = "Rec"
            };

            var created = new DoctorFinding
            {
                FindingId = 1,
                AppointmentId = dto.AppointmentId,
                Observations = dto.Observations,
                Recommendations = dto.Recommendations,
                CreatedAt = DateTime.UtcNow
            };

            _repoMock
                .Setup(x => x.AddAsync(It.IsAny<DoctorFinding>()))
                .ReturnsAsync(created);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.AppointmentId.Should().Be(dto.AppointmentId);
        }

<<<<<<< HEAD

=======
        // ================= GET ALL =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task GetAll_Should_Return_List()
        {
            _repoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<DoctorFinding> { new(), new() });

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(2);
        }

<<<<<<< HEAD
=======
        // ================= GET BY ID =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task GetById_Should_Return_When_Exists()
        {
            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new DoctorFinding { FindingId = 1 });

            var result = await _service.GetByIdAsync(1);

            result.FindingId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_Should_Throw_When_Not_Found()
        {
            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((DoctorFinding?)null);

            Func<Task> act = async () => await _service.GetByIdAsync(1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Finding not found");
        }

<<<<<<< HEAD
=======
        // ================= GET BY APPOINTMENT =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task GetByAppointment_Should_Return_List()
        {
            _repoMock
                .Setup(x => x.GetByAppointmentIdAsync(1))
                .ReturnsAsync(new List<DoctorFinding> { new(), new() });

            var result = await _service.GetByAppointmentAsync(1);

            result.Should().HaveCount(2);
        }

<<<<<<< HEAD
=======
        // ================= UPDATE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task Update_Should_Update_When_Exists()
        {
            var finding = new DoctorFinding
            {
                FindingId = 1,
                Observations = "Old"
            };

            var dto = new UpdateDoctorFindingDto
            {
                Observations = "New",
                Recommendations = "Updated"
            };

            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(finding);

            _repoMock.Setup(x => x.UpdateAsync(finding))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(1, dto);

            result.Observations.Should().Be("New");
        }

        [Fact]
        public async Task Update_Should_Throw_When_Not_Found()
        {
            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((DoctorFinding?)null);

            var dto = new UpdateDoctorFindingDto();

            Func<Task> act = async () => await _service.UpdateAsync(1, dto);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Finding not found");
        }

<<<<<<< HEAD
=======
        // ================= DELETE =================
>>>>>>> 2b063e61420f3c1a2515de62c29ccecde3d9689e

        [Fact]
        public async Task Delete_Should_Delete_When_Exists()
        {
            var finding = new DoctorFinding { FindingId = 1 };

            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(finding);

            _repoMock.Setup(x => x.DeleteAsync(finding))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(1);

            _repoMock.Verify(x => x.DeleteAsync(finding), Times.Once);
        }

        [Fact]
        public async Task Delete_Should_Throw_When_Not_Found()
        {
            _repoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((DoctorFinding?)null);

            Func<Task> act = async () => await _service.DeleteAsync(1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Finding not found");
        }
    }
}