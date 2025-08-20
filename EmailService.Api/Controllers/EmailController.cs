using Microsoft.AspNetCore.Mvc;
using Upcard.EmailService.Api.Models;
using Upcard.EmailService.Api.Services;

namespace Upcard.EmailService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] EmailRequest request)
    {
        try
        {
            _logger.LogInformation($"Received request to send email to {request?.ToEmail}");

            if (request == null)
            {
                _logger.LogWarning("Null request received");
                return BadRequest("Request cannot be null");
            }

            var result = await _emailService.SendEmailAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation($"Email sent successfully to {request.ToEmail}");
                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            else
            {
                _logger.LogWarning($"Failed to send email: {result.ErrorDetails}");
                return StatusCode(500, new
                {
                    success = false,
                    message = result.Message,
                    error = result.ErrorDetails
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in EmailController");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error",
                error = ex.Message
            });
        }
    }
}
