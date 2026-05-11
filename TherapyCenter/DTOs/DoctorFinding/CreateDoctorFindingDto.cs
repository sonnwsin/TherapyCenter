using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.DoctorFinding
{
    public class CreateDoctorFindingDto
    {
        [Required]
        public int AppointmentId { get; set; }

        public string? Observations { get; set; }

        public string? Recommendations { get; set; }

        public DateOnly? NextSessionDate { get; set; }
    }
}