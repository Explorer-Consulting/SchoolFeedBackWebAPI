using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using ULID = NUlid.Ulid;

namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public interface IAggregateEntityRepository<TAggregateDocument> where TAggregateDocument : AggregateEntity
    {
        Task StoreAggregateEntity(TAggregateDocument aggregateDocument);
        Task RemoveAggregateEntity(ULID aggregateId);
        Task<TAggregateDocument> RetrieveAggregateEntity(ULID aggregateId);
        Task UpdateAggregateEntity(TAggregateDocument aggregateDocument);
        Task<IReadOnlyList<TAggregateDocument>> RetrieveAllAggregateEntities(int pageSize);

    }
}
