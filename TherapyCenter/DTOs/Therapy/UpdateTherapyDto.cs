using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Therapy
{
    public class UpdateTherapyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public decimal Cost { get; set; }
    }
}