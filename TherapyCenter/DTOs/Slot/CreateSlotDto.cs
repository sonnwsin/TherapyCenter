using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Slot
{
    public class CreateSlotDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }
    }
}