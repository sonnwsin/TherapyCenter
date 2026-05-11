using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TherapyCenter.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }


        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int TherapyId { get; set; }
        public Therapy Therapy { get; set; }


        public int? ReceptionistId { get; set; }

        [ForeignKey("ReceptionistId")]
        public User Receptionist { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Status { get; set; } = "Scheduled";

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public DoctorFinding? DoctorFinding { get; set; }
        public Payment? Payment { get; set; }
    }
}