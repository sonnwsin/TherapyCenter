using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Appointment
{
    public class BookOnlineAppointmentDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MedicalHistory { get; set; }



        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }

        [Range(1, int.MaxValue)]
        public int TherapyId { get; set; }

        public DateOnly AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}