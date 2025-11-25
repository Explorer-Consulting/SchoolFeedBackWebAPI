using Core.DomainModels;
using Core.DomainModels.Builders;
using Core.Interfaces;
using System.Linq.Expressions;

namespace Infrastructure.Persistence
{
    public class CosmosQuestionnaireTemplateRepository : IQuestionnaireTemplateAggregateRepository
    {
        public Task ConstructAggregateInstanceAsync(Action<QuestionnaireTemplateBuilder> configure)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAggregateAsync(string aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireTemplate?> RetrieveAggregateAsync(Expression<Func<QuestionnaireTemplate, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<QuestionnaireTemplate> RetrieveAllAggregatesAsync(Expression<Func<QuestionnaireTemplate, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireTemplateBuilder> configure)
        {
            throw new NotImplementedException();
        }
    }
}
