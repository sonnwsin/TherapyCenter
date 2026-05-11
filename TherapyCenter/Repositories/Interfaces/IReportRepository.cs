using TherapyCenter.DTOs.Reports;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
