
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IWhitelistRepository
    {
        public Task<StudentWhitelist> GetStudentWhitelistAsync();

        public Task UpdateStudentWhitelistAsync(StudentWhitelist studentWhitelist);
    }
}
