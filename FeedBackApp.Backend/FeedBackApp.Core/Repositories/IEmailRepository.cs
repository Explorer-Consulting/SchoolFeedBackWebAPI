
namespace FeedBackApp.Core.Repositories
{
    public interface IEmailRepository
    {
        Task<IEnumerable<string>> GetEmailsToSend();
        Task RemoveEmailsAsync(IEnumerable<string> emails);
    }
}
