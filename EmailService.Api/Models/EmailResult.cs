namespace Upcard.EmailService.Api.Models;
public class EmailResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }

    public static EmailResult Success(string message)
    {
        return new EmailResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static EmailResult Failure(string errorMessage)
    {
        return new EmailResult
        {
            IsSuccess = false,
            Message = "Failed to send email",
            ErrorDetails = errorMessage
        };
    }
}
