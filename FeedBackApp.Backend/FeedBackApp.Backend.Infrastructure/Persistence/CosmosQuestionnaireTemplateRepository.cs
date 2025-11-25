using Core.DomainModels;
using Core.DomainModels.Builders;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Persistence
{
    public class CosmosQuestionnaireTemplateRepository(
        ApplicationDatabaseContext context,
        ILogger<CosmosQuestionnaireTemplateRepository> logger
    ) : IQuestionnaireTemplateAggregateRepository
    {
        public async Task ConstructAggregateInstanceAsync(Action<QuestionnaireTemplateBuilder> configure)
        {
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            var templateBuilderInstance = new QuestionnaireTemplateBuilder();
            configure(templateBuilderInstance);

            var aggregate = await templateBuilderInstance.BuildAggregateAsync();
            await context.QuestionnaireTemplates.AddAsync(aggregate);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAggregateAsync(string aggregateId)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("Aggregate ID is required.", nameof(aggregateId));

            var aggregate = await context.QuestionnaireTemplates
                .FirstOrDefaultAsync(template => template.QuestionnaireTemplateBusinessID == aggregateId);

            if (aggregate is null)
                return;

            context.QuestionnaireTemplates.Remove(aggregate);
            await context.SaveChangesAsync();
        }

        public async Task<QuestionnaireTemplate?> RetrieveAggregateAsync(
            Expression<Func<QuestionnaireTemplate, bool>> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return await context
                .QuestionnaireTemplates
                .Where(predicate)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public IAsyncEnumerable<QuestionnaireTemplate> RetrieveAllAggregatesAsync(
            Expression<Func<QuestionnaireTemplate, bool>>? predicate = null)
        {
            IQueryable<QuestionnaireTemplate> query = context
                .QuestionnaireTemplates
                .AsNoTracking();

            if (predicate is not null)
                query = query.Where(predicate);

            return query.AsAsyncEnumerable();
        }

        public async Task UpdateAggregateAsync(string aggregateId, Action<QuestionnaireTemplate> applyChanges)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("Aggregate ID is required.", nameof(aggregateId));

            if (applyChanges is not null)
            {
                var existing = await context.QuestionnaireTemplates
                    .FirstOrDefaultAsync(t => t.QuestionnaireTemplateBusinessID == aggregateId) ?? throw new InvalidOperationException(
                        $"Aggregate '{aggregateId}' was not found.");
                applyChanges(existing);

                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentNullException(nameof(applyChanges));
            }
        }
    }
}
