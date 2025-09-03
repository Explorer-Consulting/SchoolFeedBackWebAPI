
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IEmailRepository
    {
        Task<EmailsToSend?> GetEmailsDocumentAsync();
        Task UpdateEmailsDocumentAsync(EmailsToSend doc);
    }
}
