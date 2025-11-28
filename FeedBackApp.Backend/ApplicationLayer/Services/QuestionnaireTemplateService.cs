using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public sealed class QuestionnaireTemplateService(IQuestionnaireTemplateAggregateRepository aggregateRepository) : IAggregateService<QuestionnaireTemplateDTO>
    {
        public Task ConstructAggreateInstanceAsync(Action<QuestionnaireTemplateDTO> configure)
        {
            aggregateRepository.ConstructAggregateInstance(configure);
        }

        public Task DeleteAggregateAsync(string aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireTemplateDTO?> RetrieveAggregateAsync(System.Linq.Expressions.Expression<Func<QuestionnaireTemplateDTO, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<QuestionnaireTemplateDTO> RetrieveAllAggregatesAsync(System.Linq.Expressions.Expression<Func<QuestionnaireTemplateDTO, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireTemplateDTO> configure)
        {
            throw new NotImplementedException();
        }
    }
}
