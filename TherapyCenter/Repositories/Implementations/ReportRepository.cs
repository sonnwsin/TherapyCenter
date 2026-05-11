using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.DTOs.Reports;
using TherapyCenter.Repositories.Interfaces;

namespace TherapyCenter.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            // One count at a time — easy to follow in a viva (no parallel queries).
            var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            var totalAppointments = await _context.Appointments.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            var totalDoctors = await _context.Doctors.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            var totalTherapies = await _context.Therapies.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);

            return new ReportsSummaryDto
            {
                TotalUsers = totalUsers,
                TotalAppointments = totalAppointments,
                TotalDoctors = totalDoctors,
                TotalTherapies = totalTherapies
            };
        }
    }
}
