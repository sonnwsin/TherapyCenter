namespace TherapyCenter.DTOs.Doctor
{
    public class DoctorResponseDto
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Specialization { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? AvailableDays { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}