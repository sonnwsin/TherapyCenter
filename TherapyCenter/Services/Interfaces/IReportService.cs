using TherapyCenter.DTOs.Reports;

namespace TherapyCenter.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
