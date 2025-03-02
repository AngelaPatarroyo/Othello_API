using Microsoft.AspNetCore.Mvc;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(EmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail()
    {
        _logger.LogInformation("Received request to send a test email.");

        try
        {
            await _emailService.SendEmailAsync("angela.melbynrojo@gmail.com", "Test Email", "This is a test email.");
            
            _logger.LogInformation("Test email sent successfully.");
            return Ok(new { message = "Email Sent Successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email.");
            return StatusCode(500, new { message = "Failed to send email." });
        }
    }
}
