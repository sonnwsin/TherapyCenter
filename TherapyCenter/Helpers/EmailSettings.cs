namespace TherapyCenter.Helpers
{
    /// <summary>SMTP credentials and host (bound from appsettings.json <c>EmailSettings</c>).</summary>
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
    }
}
