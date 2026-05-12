using FluentAssertions;
using Moq;
using TherapyCenter.DTOs.Reports;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;

namespace TherapyCenter.Tests.Services
{
    public class ReportServiceTests
    {
        [Fact]
        public async Task GetSummaryAsync_Should_Return_Summary_Counts()
        {
            // Arrange
            var repositoryMock = new Mock<IReportRepository>();
            var expected = new ReportsSummaryDto
            {
                TotalUsers = 10,
                TotalAppointments = 5,
                TotalDoctors = 3,
                TotalTherapies = 4
            };

            repositoryMock
                .Setup(repo => repo.GetSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var service = new ReportService(repositoryMock.Object);

            // Act
            var result = await service.GetSummaryAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
