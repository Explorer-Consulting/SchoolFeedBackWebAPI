
namespace Application.Services.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, string? attachmentPath = null);
}
