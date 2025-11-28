using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public sealed class QuestionnaireResponseService(ILogger<QuestionnaireResponseService> logger, IQuestionnaireResponseAggregateRepository aggregateRepository) : IAggregateService<QuestionnaireResponseDTO>
    {
        public async Task ConstructAggreateInstanceAsync(Action<QuestionnaireResponseDTO> configure)
        {
            await aggregateRepository.ConstructAggregateInstanceAsync(configure);
        }

        public async Task DeleteAggregateAsync(string aggregateId)
        {
            logger.LogInformation("[Service] Deleting QuestionnaireResponse aggregate with ID={AggregateId}", aggregateId);
            await aggregateRepository.DeleteAggregateAsync(aggregateId);
        }

        public async Task<QuestionnaireResponseDTO?> RetrieveAggregateAsync(Expression<Func<QuestionnaireResponseDTO, bool>> predicate)
        {
            await aggregateRepository.RetrieveAggregateAsync(predicate);
        }

        public async IAsyncEnumerable<QuestionnaireResponseDTO> RetrieveAllAggregatesAsync(Expression<Func<QuestionnaireResponseDTO, bool>>? predicate = null)
        {
            await aggregateRepository.RetrieveAllAggregatesAsync(predicate);
        }

        public async Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireResponseDTO> configure)
        {
            await aggregateRepository.UpdateAggregateAsync(aggregateId, configure);
        }
    }
}
