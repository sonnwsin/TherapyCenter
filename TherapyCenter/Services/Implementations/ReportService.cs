using TherapyCenter.DTOs.Reports;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default) =>
            _reportRepository.GetSummaryAsync(cancellationToken);
    }
}
