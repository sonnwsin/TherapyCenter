namespace TherapyCenter.DTOs.Doctor
{
    public class DoctorListItemDto
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        public string Specialization { get; set; }
        public string AvailableDays { get; set; }

        public string StartTime { get; set; }  
        public string EndTime { get; set; }
    }
}