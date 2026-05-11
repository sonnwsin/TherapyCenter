using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TherapyCenter.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        public string? Bio { get; set; }

        public string? AvailableDays { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }


        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Slot>? Slots { get; set; }
    }
}