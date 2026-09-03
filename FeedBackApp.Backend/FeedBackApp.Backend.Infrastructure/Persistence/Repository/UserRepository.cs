using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model.UserIdentityModels;
using FeedBackApp.Core.Repositories;
using NUlid;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class UserRepository(AppDBContext context) : IUserRepository
    {
        public Task RemoveAllUsers(IEnumerable<Ulid> userIds)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUser(Ulid userId)
        {
            throw new NotImplementedException();
        }

        public Task<User> RetrieveUser(Ulid userId)
        {
            throw new NotImplementedException();
        }

        public Task UpsertUser(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
