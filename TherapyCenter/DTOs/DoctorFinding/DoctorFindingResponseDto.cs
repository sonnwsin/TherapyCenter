namespace TherapyCenter.DTOs.DoctorFinding
{
    public class DoctorFindingResponseDto
    {
        public int FindingId { get; set; }

        public int AppointmentId { get; set; }

        public string? Observations { get; set; }

        public string? Recommendations { get; set; }

        public DateOnly? NextSessionDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}