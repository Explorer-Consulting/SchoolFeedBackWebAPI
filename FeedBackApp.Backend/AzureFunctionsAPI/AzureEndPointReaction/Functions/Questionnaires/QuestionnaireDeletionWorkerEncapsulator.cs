using Application.DTOs.Questionnaire;
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
    public sealed class QuestionnaireDeletionWorkerEncapsulator(IQuestionnaireService service, ILogger<QuestionnaireDeletionWorkerEncapsulator> logger) :IQuestionnaireWorker
    {
        private readonly IQuestionnaireService _service = service;
        private readonly ILogger<QuestionnaireDeletionWorkerEncapsulator> _logger = logger;

        [RequireAdmin]
        [Function("PerformQuestionnaireDeletion")]
        [OpenApiOperation(
            operationId: "PerformQuestionnaireDeletion",
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
            bodyType: typeof(DeletionResponseDTO)
        )]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseData> ExecuteTaskAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "questionnaires/{id:guid}")] HttpRequestData request,
            Guid id)
        {
            try
            {

                DeletionResponseDTO result = await _service.DeleteSurveyAsync(id);

                if (!result.Success)
                {
                    var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteAsJsonAsync(result);
                    return notFound;
                }

                var ok = request.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(result);
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting questionnaire");

                var error = request.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new DeletionResponseDTO
                (
                    false,
                    $"Error deleting questionnaire: {ex.Message}"
                ));
                return error;
            }
        }
    }
}
