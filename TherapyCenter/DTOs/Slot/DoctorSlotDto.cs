namespace TherapyCenter.DTOs.Slot
{
    public class DoctorSlotDto
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = "Available";
    }
}
