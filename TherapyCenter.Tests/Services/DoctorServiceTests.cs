using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.Doctor;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _service = new DoctorService(
                _doctorRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateDoctorAsync_Should_Create_Doctor_When_Email_Is_New()
        {
            // Arrange
            var dto = new CreateDoctorDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "doctor@test.com",
                Password = "password123",
                PhoneNumber = "9999999999",
                Specialization = "Speech",
                Bio = "Experienced doctor",
                AvailableDays = "Mon-Fri",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            };

            var createdUser = new User
            {
                UserId = 1,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = "Doctor"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _doctorRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Doctor>()))
                .ReturnsAsync((Doctor doctor) =>
                {
                    doctor.DoctorId = 10;
                    return doctor;
                });

            // Act
            var result = await _service.CreateDoctorAsync(dto);

            // Assert
            result.DoctorId.Should().Be(10);
            result.Email.Should().Be(dto.Email);
            result.Specialization.Should().Be(dto.Specialization);
        }

        [Fact]
        public async Task CreateDoctorAsync_Should_Throw_When_Email_Already_Exists()
        {
            // Arrange
            var dto = new CreateDoctorDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@test.com",
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(dto.Email))
                .ReturnsAsync(new User { Email = dto.Email });

            // Act
            Func<Task> act = async () => await _service.CreateDoctorAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Email already exists");
        }

        [Fact]
        public async Task GetDoctorByIdAsync_Should_Return_Doctor_When_Found()
        {
            // Arrange
            _doctorRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Doctor
                {
                    DoctorId = 1,
                    Specialization = "Speech",
                    User = new User
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        Email = "jane@test.com",
                        Role = "Doctor"
                    }
                });

            // Act
            var result = await _service.GetDoctorByIdAsync(1);

            // Assert
            result.DoctorId.Should().Be(1);
            result.FullName.Should().Be("Jane Doe");
        }

        [Fact]
        public async Task UpdateDoctorAsync_Should_Update_Doctor_Profile()
        {
            // Arrange
            var doctor = new Doctor
            {
                DoctorId = 1,
                Specialization = "Old",
                User = new User
                {
                    FirstName = "Old",
                    LastName = "Name",
                    Email = "doctor@test.com",
                    Role = "Doctor"
                }
            };

            var dto = new UpdateDoctorDto
            {
                FirstName = "New",
                LastName = "Name",
                PhoneNumber = "8888888888",
                Specialization = "Updated",
                AvailableDays = "Tue-Thu",
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(16, 0)
            };

            _doctorRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            // Act
            var result = await _service.UpdateDoctorAsync(1, dto);

            // Assert
            result.FullName.Should().Be("New Name");
            result.Specialization.Should().Be("Updated");
            _doctorRepositoryMock.Verify(repo => repo.UpdateAsync(doctor), Times.Once);
        }

        [Fact]
        public async Task DeleteDoctorAsync_Should_Delete_When_Found()
        {
            // Arrange
            var doctor = new Doctor { DoctorId = 1 };

            _doctorRepositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            // Act
            await _service.DeleteDoctorAsync(1);

            // Assert
            _doctorRepositoryMock.Verify(repo => repo.DeleteAsync(doctor), Times.Once);
        }
    }
}
