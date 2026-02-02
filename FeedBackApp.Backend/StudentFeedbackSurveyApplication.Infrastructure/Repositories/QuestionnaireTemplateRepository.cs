using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using StudentFeedbackSurveyApplication.Infrastructure.Contexts;
using StudentFeedbackSurveyApplication.Infrastructure.Exceptions;
using System.Net;
using ULID = NUlid.Ulid;

namespace StudentFeedbackSurveyApplication.Infrastructure.Repositories
{
    public sealed class QuestionnaireTemplateRepository(
        ApplicationDatabaseContext context,
        ILogger<QuestionnaireTemplateRepository> logger) : IQuestionnaireTemplateRepository
    {
        public async Task StoreAggregateEntity(QuestionnaireTemplateDocument aggregateDocument)
        {
            ArgumentNullException.ThrowIfNull(aggregateDocument);

            try
            {
                context.QuestionnaireTemplateCollection.Add(aggregateDocument);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.Conflict })
            {
                throw new AggregateConflictException(
                    $"Aggregate with id '{aggregateDocument.Id}' already exists.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.PreconditionFailed })
            {
                throw new AggregateUpdateFailedException(
                    $"Concurrency conflict while storing aggregate '{aggregateDocument.Id}'.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.BadRequest })
            {
                throw new AggregateValidationException(
                    "Invalid aggregate document or partition key.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized
                })
            {
                throw new AggregateForbiddenException(
                    "Not authorized to write to Cosmos DB.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException cosmos &&
                      cosmos.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new AggregateThrottledException(
                    "Cosmos DB throttled the request.",
                    cosmos.RetryAfter,
                    ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.RequestTimeout
                              or HttpStatusCode.InternalServerError
                              or HttpStatusCode.ServiceUnavailable
                              or HttpStatusCode.GatewayTimeout
                })
            {
                throw new AggregateTransientFailureException(
                    "Transient Cosmos DB failure occurred.", ex);
            }
            catch (Exception ex)
            {
                throw new AggregateCreationFailedException(
                    "Failed to store aggregate.", ex);
            }
        }

        public async Task<QuestionnaireTemplateDocument> RetrieveAggregateEntity(ULID aggregateId)
        {
            try
            {
                var entity = await context.QuestionnaireTemplateCollection.FindAsync(aggregateId) ?? throw new AggregateNotFoundException(
                        $"Aggregate '{aggregateId}' not found.");
                context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AggregateNotFoundException(
                    $"Aggregate '{aggregateId}' not found.", ex);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new AggregateThrottledException(
                    "Cosmos DB throttled the request.", ex.RetryAfter, ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.RequestTimeout
                              or HttpStatusCode.InternalServerError
                              or HttpStatusCode.ServiceUnavailable
                              or HttpStatusCode.GatewayTimeout
                })
            {
                throw new AggregateTransientFailureException(
                    "Transient Cosmos DB failure occurred.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to retrieve aggregate '{AggregateId}'.", aggregateId);

                throw new AggregateRetrievalFailedException(
                    $"Failed to retrieve aggregate '{aggregateId}'.", ex);
            }
        }


        public async Task UpdateAggregateEntity(QuestionnaireTemplateDocument aggregateDocument)
        {
            ArgumentNullException.ThrowIfNull(aggregateDocument);

            try
            {
                context.Attach(aggregateDocument);
                context.Entry(aggregateDocument).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.PreconditionFailed })
            {
                throw new AggregateUpdateFailedException(
                    $"Concurrency conflict while updating aggregate '{aggregateDocument.Id}'.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.NotFound })
            {
                throw new AggregateNotFoundException(
                    $"Aggregate '{aggregateDocument.Id}' not found.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.BadRequest })
            {
                throw new AggregateValidationException(
                    "Invalid aggregate document or partition key.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized
                })
            {
                throw new AggregateForbiddenException(
                    "Not authorized to write to Cosmos DB.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException cosmos &&
                      cosmos.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new AggregateThrottledException(
                    "Cosmos DB throttled the request.",
                    cosmos.RetryAfter,
                    ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.RequestTimeout
                              or HttpStatusCode.InternalServerError
                              or HttpStatusCode.ServiceUnavailable
                              or HttpStatusCode.GatewayTimeout
                })
            {
                throw new AggregateTransientFailureException(
                    "Transient Cosmos DB failure occurred.", ex);
            }
            catch (Exception ex)
            {
                throw new AggregateUpdateFailedException(
                    $"Failed to update aggregate '{aggregateDocument.Id}'.", ex);
            }
        }


        public async Task RemoveAggregateEntity(ULID aggregateId)
        {
            try
            {
                var stub = new QuestionnaireTemplateDocument
                {
                    Id = aggregateId,
                    Title = string.Empty,
                    Description = null,
                    SelfEnrollmentAllowed = false,
                    StartDate = default,
                    EndDate = default,
                    CategorySections = []
                };

                context.Entry(stub).State = EntityState.Deleted;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.NotFound })
            {
                throw new AggregateNotFoundException($"Aggregate '{aggregateId}' not found.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized })
            {
                throw new AggregateForbiddenException("Not authorized to write to Cosmos DB.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException cosmos && cosmos.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new AggregateThrottledException("Cosmos DB throttled the request.", cosmos.RetryAfter, ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException
            {
                StatusCode: HttpStatusCode.RequestTimeout
                          or HttpStatusCode.InternalServerError
                          or HttpStatusCode.ServiceUnavailable
                          or HttpStatusCode.GatewayTimeout
            })
            {
                throw new AggregateTransientFailureException("Transient Cosmos DB failure occurred.", ex);
            }
        }

        public async Task<IReadOnlyList<QuestionnaireTemplateDocument>> RetrieveAllAggregateEntities()
        {
            return await context.QuestionnaireTemplateCollection
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
