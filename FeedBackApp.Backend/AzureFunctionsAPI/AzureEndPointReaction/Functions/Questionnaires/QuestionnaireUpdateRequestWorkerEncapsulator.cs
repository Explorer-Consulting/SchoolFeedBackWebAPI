using Application.DTOs;
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
    public sealed class QuestionnaireUpdateRequestWorkerEncapsulator(IEvaluationService service, ILogger<QuestionnaireUpdateRequestWorkerEncapsulator> logger) : IQuestionnaireWorker
    {
        private readonly IEvaluationService _service = service;
        private readonly ILogger<QuestionnaireUpdateRequestWorkerEncapsulator> _logger = logger;

        [RequireStudent]
        [Function("PerformQuestionnaireUpdate")]
        [OpenApiOperation(
            operationId: "PerformQuestionnaireUpdate",
            tags: new[] { "Evaluations" }
        )]
        [OpenApiParameter(
            name: "id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(Guid)
        )]
        [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(object), // replace with update DTO
            Required = true
        )]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(UpdateResponseDTO)
        )]
        public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "evaluations/{id:guid}")] HttpRequestData request, Guid id)
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                var emptyResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                await emptyResponse.WriteAsJsonAsync(new UpdateResponseDTO {Success=false, Message = "Request body cannot be empty." });
                return emptyResponse;
            }
            /*implementation in progress*/
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new UpdateResponseDTO {Success=true, Message = "Update successful" });
            return response;

        }
    }
}
