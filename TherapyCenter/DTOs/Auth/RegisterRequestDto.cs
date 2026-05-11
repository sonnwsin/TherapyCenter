using System.ComponentModel.DataAnnotations;

namespace TherapyCenter.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Receptionist, Doctor, Guardian

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}