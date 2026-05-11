namespace TherapyCenter.DTOs.Reports
{
    /// <summary>Minimal admin dashboard: four totals from the database.</summary>
    public class ReportsSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalTherapies { get; set; }
    }
}
