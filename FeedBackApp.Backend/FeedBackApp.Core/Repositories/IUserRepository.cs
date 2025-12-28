using FeedBackApp.Core.Model.UserIdentityModels;
using NUlid;

namespace FeedBackApp.Core.Repositories
{
    public interface IUserRepository
    {
        public Task UpsertUser(User entity);
        public Task RemoveUser(Ulid userId);
        public Task<User> RetrieveUser(Ulid userId);
        public Task RemoveAllUsers(IEnumerable<Ulid> userIds);
    }
}
