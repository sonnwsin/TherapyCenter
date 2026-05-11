using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Appointment
{
    public class CreateWalkInAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }

        [Range(1, int.MaxValue)]
        public int TherapyId { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}