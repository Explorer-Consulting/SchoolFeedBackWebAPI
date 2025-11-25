using Core.DomainModels;
using Core.DomainModels.Builders;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance
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
