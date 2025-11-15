using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;


namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplateDeletionRequestEvent(ILogger<QuestionnaireTemplateDeletionRequestEvent> logger)
    {
        [Function(nameof(QuestionnaireTemplateDeletionRequestEvent))]
        [OpenApiOperation(
            operationId: "QuestionnaireTemplateDeletion",
            tags: ["Questionnaire Templates"],
            Summary = "Delete questionnaire template",
            Description = "Deletes a questionnaire template identified by its unique ID if it exists.")]
        [OpenApiParameter(
            name: "questionnaireTemplateId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Unique identifier of the questionnaire template to delete",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaireTemplateId}.")]
        [OpenApiSecurity(
            "function_key",
            SecuritySchemeType.ApiKey,
            Name = "x-functions-key",
            In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NoContent,
            Summary = "Deleted",
            Description = "The questionnaire template has been successfully deleted.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NotFound,
            Summary = "Not Found",
            Description = "No questionnaire template was found for the given identifier.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.Unauthorized,
            Summary = "Unauthorized",
            Description = "Missing or invalid authentication credentials.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NotImplemented,
            Summary = "Not Implemented",
            Description = "The delete functionality is not yet implemented.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "DELETE", Route = "v1/questionnaire-templates/{questionnaire-template-id}")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var response = request.CreateResponse(HttpStatusCode.NotImplemented);
            await response.WriteStringAsync($"Delete questionnaire template '{questionnaireTemplateId}' - not implemented yet.");
            return response;
        }
    }
}
