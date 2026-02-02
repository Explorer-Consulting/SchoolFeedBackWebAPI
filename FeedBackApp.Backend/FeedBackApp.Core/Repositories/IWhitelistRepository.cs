
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IWhitelistRepository
    {
        Task<IReadOnlyList<string>> GetStudentEmailsAsync(string id = "StudentWhitelist", CancellationToken ct = default);

        public Task<StudentWhitelist> GetStudentWhitelistAsync();
        public Task UpdateStudentWhitelistAsync(StudentWhitelist studentWhitelist);
    }
}
