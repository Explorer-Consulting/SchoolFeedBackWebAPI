using NUlid;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public class AggregateRepository<TAggregateEntity> : IAggregateEntityRepository<TAggregateEntity> where TAggregateEntity : AggregateEntity
    {
        public Task RemoveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateEntity> RetrieveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TAggregateEntity>> RetrieveAllAggregateEntities()
        {
            throw new NotImplementedException();
        }

        public Task StoreAggregateEntity(TAggregateEntity aggregateDocument)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateEntity(TAggregateEntity aggregateDocument)
        {
            throw new NotImplementedException();
        }
    }
}
