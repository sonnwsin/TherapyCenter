using System;
using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.Models
{
    public class DoctorFinding
    {
        [Key]
        public int FindingId { get; set; }


        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public string? Observations { get; set; }

        public string? Recommendations { get; set; }

        public DateOnly? NextSessionDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}