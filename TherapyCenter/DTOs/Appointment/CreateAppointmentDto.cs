using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TherapyId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SlotId { get; set; }

        public int? ReceptionistId { get; set; }

        public string? Notes { get; set; }
    }
}