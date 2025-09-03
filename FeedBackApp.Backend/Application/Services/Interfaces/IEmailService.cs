
namespace Application.Services.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailBatchAsync();
}
