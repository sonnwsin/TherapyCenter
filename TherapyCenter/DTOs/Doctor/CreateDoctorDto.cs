using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Doctor
{
    public class CreateDoctorDto
    {

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }


        [Required]
        public string Specialization { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? AvailableDays { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}