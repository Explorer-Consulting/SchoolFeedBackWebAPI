using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ApplicationLayer.Services
{
    public sealed class QuestionnaireTemplateService(ILogger<QuestionnaireTemplateService> logger, IQuestionnaireTemplateAggregateRepository aggregateRepository) : IAggregateService<QuestionnaireTemplateDTO>
    {
        public async Task ConstructAggreateInstanceAsync(Action<QuestionnaireTemplateDTO> configure)
        {
            //validators and other business logic can be added here before constructing the aggregate
            
            logger.LogInformation("[Service] Constructing a new QuestionnaireTemplate aggregate instance.");
            await aggregateRepository.ConstructAggregateInstanceAsync(configure);
            logger.LogInformation("[Service] QuestionnaireTemplate aggregate instance constructed successfully.");
        }

        public async Task DeleteAggregateAsync(string aggregateId)
        {
            // validators and other business logic can be added here before deleting the aggregate
            await aggregateRepository.DeleteAggregateAsync(aggregateId);
        }

        public async Task<QuestionnaireTemplateDTO?> RetrieveAggregateAsync(Expression<Func<QuestionnaireTemplateDTO, bool>> predicate)
        {
            // validators and other business logic can be added here before retrieving the aggregate
            await aggregateRepository.RetrieveAggregateAsync(predicate);
        }

        public async IAsyncEnumerable<QuestionnaireTemplateDTO> RetrieveAllAggregatesAsync(Expression<Func<QuestionnaireTemplateDTO, bool>>? predicate = null)
        // validators and other business logic can be added here before retrieving the aggregates
        {
            await await aggregateRepository.RetrieveAllAggregatesAsync(predicate);
        }

        public async Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireTemplateDTO> configure)
        {
            // validators and other business logic can be added here before updating the aggregate
            await aggregateRepository.UpdateAggregateAsync(aggregateId, configure);
        }
    }
}
