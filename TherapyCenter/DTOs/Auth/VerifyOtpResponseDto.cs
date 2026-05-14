namespace TherapyCenter.DTOs.Auth
{
    /// <summary>Outcome of comparing the submitted OTP with the value stored in Redis.</summary>
    public class VerifyOtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>valid | invalid | expired</summary>
        public string Status { get; set; } = string.Empty;
    }
}
