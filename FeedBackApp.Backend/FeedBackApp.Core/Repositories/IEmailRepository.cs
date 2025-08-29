
namespace FeedBackApp.Core.Repositories
{
    public interface IEmailRepository
    {
        Task<IEnumerable<string>> GetEmailsToSend();
    }
}
