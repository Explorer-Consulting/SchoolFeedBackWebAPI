using Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly string _appPassword;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _fromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not set.");
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? throw new InvalidOperationException("EMAIL_FROM_NAME is not set.");
        _appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") ?? throw new InvalidOperationException("EMAIL_APP_PASSWORD is not set.");
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, string? attachmentPath = null)
    {
        try
        {
            var from = new MailAddress(_fromAddress, _fromName);
            var to = new MailAddress(toEmail, toName);

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_fromAddress, _appPassword),
                EnableSsl = true
            };

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                message.Attachments.Add(new Attachment(attachmentPath));
            }

            await smtp.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {toEmail}");
            return false;
        }
    }
}
