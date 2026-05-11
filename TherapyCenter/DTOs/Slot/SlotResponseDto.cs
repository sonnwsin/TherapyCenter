namespace TherapyCenter.DTOs.Slot
{
    public class SlotResponseDto
    {
        public int SlotId { get; set; }

        public int DoctorId { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}