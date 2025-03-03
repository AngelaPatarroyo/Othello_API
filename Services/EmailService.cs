using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        // Fetch SMTP settings from environment variables first, fallback to appsettings.json
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? _configuration["EmailSettings:SmtpServer"];
        var smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? _configuration["EmailSettings:SmtpPort"];
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? _configuration["EmailSettings:SmtpUsername"];
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _configuration["EmailSettings:SmtpPassword"];

        // Validate SMTP settings
        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            _logger.LogError("SMTP settings are missing. Server: {Server}, User: {User}", smtpServer, smtpUser);
            throw new InvalidOperationException("SMTP settings are missing from configuration or environment variables.");
        }

        if (!int.TryParse(smtpPort, out int port))
        {
            _logger.LogError("SMTP Port is not a valid number: {Port}", smtpPort);
            throw new InvalidOperationException("SMTP Port is not a valid number.");
        }

        try
        {
            _logger.LogInformation("Connecting to SMTP server {Server}:{Port} as {User}", smtpServer, port, smtpUser);

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;  // Ensures only provided credentials are used
                client.DeliveryMethod = SmtpDeliveryMethod.Network;  // Uses network SMTP
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation(" Email sent successfully to {EmailAddress}.", to);
            }
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, " SMTP error occurred while sending email to {EmailAddress}. Error: {Error}", to, smtpEx.Message);
            throw new InvalidOperationException($"SMTP error: {smtpEx.Message}", smtpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " An unexpected error occurred while sending email to {EmailAddress}.", to);
            throw new InvalidOperationException("An unexpected error occurred while sending the email.", ex);
        }
    }
}