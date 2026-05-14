using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TherapyCenter.Helpers;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSettings settings, ILogger<EmailService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.Email) ||
                string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogWarning("EmailSettings is incomplete; cannot send mail to {To}", toEmail);
                throw new InvalidOperationException(
                    "Email is not configured. Add EmailSettings (Host, Email, Password, Port) to appsettings.json.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Therapy Center", _settings.Email));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls)
                    .ConfigureAwait(false);
                await client.AuthenticateAsync(_settings.Email, _settings.Password).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                _logger.LogInformation("Sent email to {To} with subject {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", toEmail);
                throw;
            }
        }
    }
}
