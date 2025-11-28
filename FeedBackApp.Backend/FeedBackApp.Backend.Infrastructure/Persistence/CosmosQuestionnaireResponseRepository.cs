using Core.DomainModels;
using Core.DomainModels.Builders;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Repository implementation for managing <see cref="QuestionnaireResponse"/> aggregates
    /// using the EF Core Cosmos provider.
    /// </summary>
    /// <remarks>
    /// This repository works with business-level identifiers for aggregates and does not expose
    /// storage-specific identifiers such as the internal storage ID in logs.
    /// </remarks>
    public class CosmosQuestionnaireResponseRepository(
        ApplicationDatabaseContext context,
        ILogger<CosmosQuestionnaireResponseRepository> logger
    ) : IQuestionnaireResponseAggregateRepository
    {
        /// <summary>
        /// Constructs and persists a new <see cref="QuestionnaireResponse"/> aggregate using a builder.
        /// </summary>
        /// <param name="configure">
        /// A delegate that configures the <see cref="QuestionnaireResponseBuilder"/> instance
        /// before the aggregate is built and saved.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
        /// </exception>
        public async Task ConstructAggregateInstanceAsync(
            Action<QuestionnaireResponseBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            logger.LogInformation("Constructing new QuestionnaireResponse aggregate...");

            var builder = new QuestionnaireResponseBuilder();
            configure(builder);

            logger.LogDebug("Building QuestionnaireResponse aggregate instance...");
            var aggregate = await builder.BuildAggregateAsync();

            using var scope = logger.BeginScope(
                "QuestionnaireResponse {BusinessID} / Template {TemplateBusinessID}",
                aggregate.QuestionnaireResponseBusinessID,
                aggregate.QuestionnaireTemplateBusinessID);

            logger.LogInformation("Saving QuestionnaireResponse aggregate...");
            await context.QuestionnaireResponses.AddAsync(aggregate);
            await context.SaveChangesAsync();

            logger.LogInformation("QuestionnaireResponse aggregate successfully created.");
        }

        /// <summary>
        /// Deletes an existing <see cref="QuestionnaireResponse"/> aggregate identified by its business ID.
        /// </summary>
        /// <param name="aggregateId">
        /// The business identifier of the aggregate to delete. Cannot be <see langword="null"/>,
        /// empty, or consist only of white-space characters.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous delete operation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="aggregateId"/> is <see langword="null"/>, empty,
        /// or consists only of white-space characters.
        /// </exception>
        public async Task DeleteAggregateAsync(string aggregateId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(aggregateId);

            logger.LogInformation(
                "Deleting QuestionnaireResponse with BusinessID={BusinessID}",
                aggregateId);

            var entity = await context.QuestionnaireResponses
                .FirstOrDefaultAsync(r => r.QuestionnaireResponseBusinessID == aggregateId);

            if (entity is null)
            {
                logger.LogWarning(
                    "QuestionnaireResponse with BusinessID={BusinessID} not found. Nothing to delete.",
                    aggregateId);
                return;
            }

            context.QuestionnaireResponses.Remove(entity);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "QuestionnaireResponse with BusinessID={BusinessID} successfully deleted.",
                aggregateId);
        }

        /// <summary>
        /// Retrieves a single <see cref="QuestionnaireResponse"/> aggregate that matches
        /// the specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// An expression used to filter the <see cref="QuestionnaireResponse"/> sequence.
        /// Cannot be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The task result contains the
        /// first matching <see cref="QuestionnaireResponse"/> instance, or <see langword="null"/>
        /// if no match is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is <see langword="null"/>.
        /// </exception>
        public Task<QuestionnaireResponse?> RetrieveAggregateAsync(
            Expression<Func<QuestionnaireResponse, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return context.QuestionnaireResponses
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Retrieves all <see cref="QuestionnaireResponse"/> aggregates, optionally filtered
        /// by the specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// An optional expression used to filter the <see cref="QuestionnaireResponse"/> sequence.
        /// If <see langword="null"/>, all aggregates are returned.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> sequence of <see cref="QuestionnaireResponse"/> aggregates
        /// that satisfy the optional filter.
        /// </returns>
        public IAsyncEnumerable<QuestionnaireResponse> RetrieveAllAggregatesAsync(
            Expression<Func<QuestionnaireResponse, bool>>? predicate = null)
        {
            IQueryable<QuestionnaireResponse> query = context.QuestionnaireResponses.AsNoTracking();

            if (predicate is not null)
            {
                query = query.Where(predicate);
            }

            return query.AsAsyncEnumerable();
        }

        /// <summary>
        /// Updates an existing <see cref="QuestionnaireResponse"/> aggregate specified by its business ID.
        /// </summary>
        /// <param name="aggregateId">
        /// The business identifier of the aggregate to update. Cannot be <see langword="null"/>,
        /// empty, or consist only of white-space characters.
        /// </param>
        /// <param name="configure">
        /// A delegate that applies domain-level mutations to the retrieved aggregate instance.
        /// Cannot be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous update operation.
        /// </returns>
        /// <remarks>
        /// If an aggregate with the specified <paramref name="aggregateId"/> does not exist,
        /// the method logs a warning and returns without applying any changes.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="aggregateId"/> is <see langword="null"/>, empty,
        /// or consists only of white-space characters.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configure"/> is <see langword="null"/>.
        /// </exception>
        public async Task UpdateAggregateAsync(
            string aggregateId,
            Action<QuestionnaireResponse> configure)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(aggregateId);
            ArgumentNullException.ThrowIfNull(configure);

            logger.LogInformation(
                "Updating QuestionnaireResponse with BusinessID={BusinessID}",
                aggregateId);

            var entity = await context.QuestionnaireResponses
                .FirstOrDefaultAsync(r => r.QuestionnaireResponseBusinessID == aggregateId);

            if (entity is null)
            {
                logger.LogWarning(
                    "QuestionnaireResponse with BusinessID={BusinessID} not found. Update aborted.",
                    aggregateId);
                return;
            }

            using var scope = logger.BeginScope(
                "Updating QuestionnaireResponse {BusinessID}",
                entity.QuestionnaireResponseBusinessID);

            configure(entity);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "QuestionnaireResponse with BusinessID={BusinessID} successfully updated.",
                aggregateId);
        }
    }
}
