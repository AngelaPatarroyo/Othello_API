using System.Net;
using System.Net.Mail;


public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Attempting to send email to {EmailAddress} with subject '{Subject}'.", to, subject);

        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = _configuration["EmailSettings:SmtpPort"];
        var smtpUser = _configuration["EmailSettings:SmtpUsername"];
        var smtpPass = _configuration["EmailSettings:SmtpPassword"];

        // Validate configuration values
        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            _logger.LogError("SMTP settings are missing from configuration.");
            throw new InvalidOperationException("SMTP settings are missing from configuration.");
        }

        if (!int.TryParse(smtpPort, out int port))
        {
            _logger.LogError("SMTP Port is not a valid number.");
            throw new InvalidOperationException("SMTP Port is not a valid number.");
        }

        try
        {
            using (var client = new SmtpClient(smtpServer, port))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {EmailAddress}.", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {EmailAddress}.", to);
            throw new InvalidOperationException("An error occurred while sending the email.", ex);
        }
    }
}
