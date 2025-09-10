
namespace Application.Services.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailBatchAsync();
    Task CompileReportEmailsAsync(Guid surveyId);
}
