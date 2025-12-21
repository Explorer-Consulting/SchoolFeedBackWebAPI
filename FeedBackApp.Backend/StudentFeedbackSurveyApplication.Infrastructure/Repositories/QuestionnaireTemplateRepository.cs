using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUlid;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using StudentFeedbackSurveyApplication.Infrastructure.Contexts;
using StudentFeedbackSurveyApplication.Infrastructure.Exceptions;
using System.Net;

namespace StudentFeedbackSurveyApplication.Infrastructure.Repositories
{
    public class QuestionnaireTemplateRepository(ApplicationDatabaseContext context, ILogger<QuestionnaireResponseRepository> logger) : IQuestionnaireTemplateRepository
    {
        public Task RemoveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireTemplateDocument> RetrieveAggregateEntity(Ulid aggregateId)
        {
            throw new NotImplementedException();
        }

        public async Task StoreAggregateEntity(QuestionnaireTemplateDocument aggregateDocument)
        {
            ArgumentNullException.ThrowIfNull(aggregateDocument);

            try
            {
                context.QuestionnaireTemplateCollection.Add(aggregateDocument);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.Conflict })
            {
                throw new AggregateConflictException(
                    $"Aggregate with id '{aggregateDocument.Id}' already exists.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.PreconditionFailed })
            {
                throw new AggregateUpdateFailedException(
                    $"Concurrency conflict while storing aggregate '{aggregateDocument.Id}'.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.NotFound })
            {
                throw new AggregateNotFoundException(
                    $"Aggregate '{aggregateDocument.Id}' not found.", ex);
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
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.BadRequest })
            {
                throw new AggregateValidationException(
                    "Invalid aggregate document or partition key.", ex);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is CosmosException
                {
                    StatusCode: HttpStatusCode.Forbidden
                              or HttpStatusCode.Unauthorized
                })
            {
                throw new AggregateForbiddenException(
                    "Not authorized to write to Cosmos DB.", ex);
            }
            catch (Exception ex)
            {
                throw new AggregateCreationFailedException(
                    "Failed to store aggregate.", ex);
            }
        }

    public Task UpdateAggregateEntity(QuestionnaireTemplateDocument aggregateDocument)
        {
            throw new NotImplementedException();
        }
    }
}
