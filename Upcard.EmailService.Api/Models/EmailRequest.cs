namespace Upcard.EmailService.Api.Models;

public class EmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public object? TemplateData { get; set; }
}
