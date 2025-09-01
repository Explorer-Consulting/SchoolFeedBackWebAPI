using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace AzureEndPointReaction.Functions.Questionnaires
{
    public sealed class QuestionnaireSummaryRequestWorkerEncapsulator(IQuestionnaireService service, ILogger<QuestionnaireSummaryRequestWorkerEncapsulator> logger) : IQuestionnaireWorker
    {
        private readonly IQuestionnaireService _service = service;
        private readonly ILogger<QuestionnaireSummaryRequestWorkerEncapsulator> _logger = logger;

        [RequireAdmin]
        [Function("PerformQuestionnarieStatistics")]
        [OpenApiOperation(
            operationId: "PerformQuestionnaireStatistics",
            tags: new[] { "Questionnaires" }
        )]
        [OpenApiParameter(
            name: "id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(Guid)
        )]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(object) // replace with `summary` DTO
        )]
        public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summaries/{id:guid}")] HttpRequestData request, Guid id)
        {
            /*implementation in progress*/
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Get successful" });
            return response;

        }
    }
}
