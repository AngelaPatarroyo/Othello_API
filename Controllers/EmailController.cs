using Microsoft.AspNetCore.Mvc;


[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail()
    {
        await _emailService.SendEmailAsync("recipient@example.com", "Test Email", "This is a test email.");
        return Ok("Email Sent Successfully!");
    }
}
