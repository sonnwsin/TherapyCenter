namespace TherapyCenter.Services.Interfaces
{
    /// <summary>
    /// Central booking rules for generated-slot style appointments (walk-in, online, and slot-based create).
    /// </summary>
    public interface IBookingValidationService
    {
        Task ValidateBookableWindowAsync(
            int doctorId,
            int therapyId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime);
    }
}
