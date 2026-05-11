using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.Models
{
    public class Therapy
    {
        public int TherapyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public decimal Cost { get; set; }


        public ICollection<Appointment>? Appointments { get; set; }
    }
}