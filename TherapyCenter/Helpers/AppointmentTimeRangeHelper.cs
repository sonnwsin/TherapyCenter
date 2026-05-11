namespace TherapyCenter.Helpers
{
    public static class AppointmentTimeRangeHelper
    {
        public static bool RangesOverlap(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd) =>
            aStart < bEnd && aEnd > bStart;

        public static bool IsCancelled(string? status) =>
            string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
    }
}
