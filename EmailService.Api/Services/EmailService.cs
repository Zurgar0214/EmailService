using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Upcard.EmailService.Api.Models;

namespace Upcard.EmailService.Api.Services
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(EmailRequest request);
    }

    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _apiKey = config["SendGrid:ApiKey"]!;
            _fromEmail = config["SendGrid:FromEmail"]!;
            _fromName = config["SendGrid:FromName"]!;
            _logger = logger;
        }

        public async Task<EmailResult> SendEmailAsync(EmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("SendGrid API Key not configured");
                    return EmailResult.Failure("SendGrid API Key not configured");
                }

                if (string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.TemplateId))
                {
                    _logger.LogError("Destination email or Template ID are required");
                    return EmailResult.Failure("Destination email or Template ID are required");
                }

                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(request.ToEmail);
                var testData = ConvertToDictionary(request.TemplateData!);

                var msg = MailHelper.CreateSingleTemplateEmail(
                    from,
                    to,
                    request.TemplateId,
                    testData
                );

                _logger.LogInformation($"Sending email to {request.ToEmail} with template {request.TemplateId}");

                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Email sent successfully to {request.ToEmail}");
                    return EmailResult.Success("Email sent successfully");
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"SendGrid error: StatusCode={response.StatusCode}, Body={errorBody}");
                    return EmailResult.Failure($"SendGrid error: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while sending email to {request.ToEmail}");
                return EmailResult.Failure($"Internal error: {ex.Message}");
            }
        }

        private Dictionary<string, object> ConvertToDictionary(object data)
        {
            if (data == null) return new Dictionary<string, object>();

            try
            {
                if (data.GetType().Name == "JsonElement")
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    data = JsonConvert.DeserializeObject(json);
                }

                var jObject = JObject.FromObject(data);

                var result = new Dictionary<string, object>();
                foreach (var prop in jObject.Properties())
                {
                    var lowerName = prop.Name.ToLowerInvariant();

                    if (prop.Value.Type == JTokenType.Object || prop.Value.Type == JTokenType.Array)
                    {
                        result[lowerName] = prop.Value.ToString(Formatting.None);
                    }
                    else
                    {
                        result[lowerName] = ((JValue)prop.Value).Value;
                    }
                }

                _logger.LogInformation("TemplateData proccessed: {Data}",
                    JsonConvert.SerializeObject(result, Formatting.Indented));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Missing parsing TemplateData");
                return [];
            }
        }
    }
}