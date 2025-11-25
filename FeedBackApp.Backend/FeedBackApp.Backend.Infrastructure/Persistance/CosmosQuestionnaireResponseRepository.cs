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
    public class CosmosQuestionnaireResponseRepository : IQuestionnaireResponseAggregateRepository
    {
        public Task ConstructAggregateInstanceAsync(Action<QuestionnaireResponseBuilder> configure)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAggregateAsync(string aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireResponse?> RetrieveAggregateAsync(Expression<Func<QuestionnaireResponse, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<QuestionnaireResponse> RetrieveAllAggregatesAsync(Expression<Func<QuestionnaireResponse, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireResponseBuilder> configure)
        {
            throw new NotImplementedException();
        }
    }
}
