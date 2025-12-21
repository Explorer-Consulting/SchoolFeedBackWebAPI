
using Microsoft.Extensions.Logging;
using NUlid;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using StudentFeedbackSurveyApplication.Infrastructure.Contexts;

namespace StudentFeedbackSurveyApplication.Infrastructure.Repositories
{
    public sealed class UserRepository(ApplicationDatabaseContext context, ILogger<UserRepository> logger) : IUserRepository
    {
        public Task RemoveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<User> RetrieveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task StoreAggregateEntity(User aggregateDocument)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateEntity(User aggregateDocument)
        {
            throw new NotImplementedException();
        }
    }
}
