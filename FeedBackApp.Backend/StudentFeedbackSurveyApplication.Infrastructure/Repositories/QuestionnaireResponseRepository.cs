using Microsoft.Extensions.Logging;
using NUlid;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using StudentFeedbackSurveyApplication.Infrastructure.Contexts;

namespace StudentFeedbackSurveyApplication.Infrastructure.Repositories
{
    public sealed class QuestionnaireResponseRepository(ApplicationDatabaseContext context, ILogger<QuestionnaireResponseRepository> logger) : IQuestionnaireResponseRepository
    {
        public Task RemoveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireResponseDocument> RetrieveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<QuestionnaireResponseDocument>> RetrieveAllAggregateEntities()
        {
            throw new NotImplementedException();
        }

        public Task StoreAggregateEntity(QuestionnaireResponseDocument aggregateDocument)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateEntity(QuestionnaireResponseDocument aggregateDocument)
        {
            throw new NotImplementedException();
        }
    }
}
