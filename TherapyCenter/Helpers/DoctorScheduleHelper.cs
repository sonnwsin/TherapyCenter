namespace TherapyCenter.Helpers
{
    public static class DoctorScheduleHelper
    {
        public static bool IsDoctorAvailableOnDate(string availableDays, DateOnly date)
        {
            var day = date.DayOfWeek;

            if (availableDays.Contains('-'))
            {
                var parts = availableDays.Split('-');

                var start = ParseDay(parts[0]);
                var end = ParseDay(parts[1]);

                return day >= start && day <= end;
            }

            return ParseDay(availableDays) == day;
        }

        public static DayOfWeek ParseDay(string day)
        {
            return day.Trim().ToLower() switch
            {
                "mon" => DayOfWeek.Monday,
                "tue" => DayOfWeek.Tuesday,
                "wed" => DayOfWeek.Wednesday,
                "thu" => DayOfWeek.Thursday,
                "fri" => DayOfWeek.Friday,
                "sat" => DayOfWeek.Saturday,
                "sun" => DayOfWeek.Sunday,
                _ => throw new Exception("Invalid day in doctor availability configuration.")
            };
        }
    }
}
