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

            logger.LogInformation("Constructing new QuestionnaireTemplate aggregate...");

            var builder = new QuestionnaireTemplateBuilder();
            configure(builder);

            logger.LogDebug("Building QuestionnaireTemplate aggregate...");

            var aggregate = await builder.BuildAggregateAsync();

            logger.LogInformation(
                "Creating aggregate with BusinessID={BusinessId}",
                aggregate.QuestionnaireTemplateBusinessID);

            await context.QuestionnaireTemplates.AddAsync(aggregate);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Aggregate successfully created. BusinessID={BusinessId}",
                aggregate.QuestionnaireTemplateBusinessID);
        }

        public async Task DeleteAggregateAsync(string aggregateId)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("Aggregate ID is required.", nameof(aggregateId));

            logger.LogInformation("Attempting to delete aggregate with BusinessID={AggregateId}", aggregateId);

            var aggregate = await context.QuestionnaireTemplates
                .FirstOrDefaultAsync(t => t.QuestionnaireTemplateBusinessID == aggregateId);

            if (aggregate is null)
            {
                logger.LogWarning("Delete aborted: aggregate not found. BusinessID={AggregateId}", aggregateId);
                return;
            }

            logger.LogDebug("Removing aggregate BusinessID={AggregateId}", aggregateId);

            context.QuestionnaireTemplates.Remove(aggregate);
            await context.SaveChangesAsync();

            logger.LogInformation("Deleted aggregate BusinessID={AggregateId}", aggregateId);
        }

        public async Task<QuestionnaireTemplate?> RetrieveAggregateAsync(
            Expression<Func<QuestionnaireTemplate, bool>> predicate)
        {
            if (predicate is not null)
            {
                logger.LogDebug("Retrieving aggregate with predicate={Predicate}", predicate);

                var result = await context.QuestionnaireTemplates
                    .Where(predicate)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (result is null)
                    logger.LogWarning("Aggregate not found for predicate={Predicate}", predicate);
                else
                    logger.LogInformation("Retrieved aggregate BusinessID={BusinessId}",
                        result.QuestionnaireTemplateBusinessID);

                return result;
            }

            throw new ArgumentNullException(nameof(predicate));
        }

        public IAsyncEnumerable<QuestionnaireTemplate> RetrieveAllAggregatesAsync(
            Expression<Func<QuestionnaireTemplate, bool>>? predicate = null)
        {
            logger.LogDebug("Retrieving all aggregates. Filter applied: {HasPredicate}", predicate is not null);

            IQueryable<QuestionnaireTemplate> query = context
                .QuestionnaireTemplates
                .AsNoTracking();

            if (predicate is not null)
            {
                logger.LogDebug("Applying filter predicate={Predicate}", predicate);
                query = query.Where(predicate);
            }

            return query.AsAsyncEnumerable();
        }

        public async Task UpdateAggregateAsync(
            string aggregateId,
            Action<QuestionnaireTemplate> applyChanges)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("Aggregate ID is required.", nameof(aggregateId));

            if (applyChanges is null)
                throw new ArgumentNullException(nameof(applyChanges));

            logger.LogInformation("Updating aggregate BusinessID={AggregateId}", aggregateId);

            var existing = await context.QuestionnaireTemplates
                .FirstOrDefaultAsync(t => t.QuestionnaireTemplateBusinessID == aggregateId)
                ?? throw new InvalidOperationException(
                    $"Aggregate '{aggregateId}' was not found.");

            logger.LogDebug("Applying changes to aggregate BusinessID={AggregateId}", aggregateId);

            applyChanges(existing);

            await context.SaveChangesAsync();

            logger.LogInformation("Successfully updated aggregate BusinessID={AggregateId}", aggregateId);
        }
    }
}
