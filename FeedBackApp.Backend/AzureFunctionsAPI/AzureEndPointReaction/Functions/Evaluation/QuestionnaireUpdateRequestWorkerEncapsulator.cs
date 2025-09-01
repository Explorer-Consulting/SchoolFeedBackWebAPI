using Application.DTOs.Evaluation;
using Application.Services.Interfaces;
using AzureFunctionsAPI.AzureEndPointReaction.Functions.Utils;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions.Evaluation
{
    public sealed class QuestionnaireUpdateRequestWorkerEncapsulator(IEvaluationService service, ILogger<QuestionnaireUpdateRequestWorkerEncapsulator> logger)
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
            Type = typeof(string)
        )]
        [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(UpdateQuestionnaireDTO),
            Required = true
        )]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(UpdateResponseDTO)
        )]
        public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "questionnaire/{id}")] HttpRequestData request, string id)
        {
            try
            {
                var dto = await JsonUtil.ReadFromJsonAsync<UpdateQuestionnaireDTO>(request);

                if (dto == null)
                {
                    _logger.LogError("Invalid or empty JSON body");
                    var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                    return badResponse;
                }

                var result = await _service.UpdateQuestionnaire(id, dto);

                if (!result.Success)
                {
                    var error = request.CreateResponse(HttpStatusCode.BadRequest);
                    await error.WriteAsJsonAsync(result);
                    return error;
                }

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError("Something unexpected happenned!", e.Message);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new UpdateResponseDTO(false, $"Error updating questionnaire: {e.Message}"));
                return response;
            }

        }
    }
}
