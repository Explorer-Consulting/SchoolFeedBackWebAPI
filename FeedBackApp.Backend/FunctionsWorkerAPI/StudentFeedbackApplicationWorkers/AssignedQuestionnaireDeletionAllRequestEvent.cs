using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class AssignedQuestionnaireDeletionAllRequestEvent
    {
        [Function(nameof(AssignedQuestionnaireDeletionAllRequestEvent))]
        [OpenApiOperation(
            operationId: "DeleteAllResponsesForQuestionnaireTemplate",
            tags: ["Assigned Questionnaires"],
            Summary = "Delete all responses for a questionnaire template",
            Description = "Deletes all responses associated with the specified questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaireTemplateId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Questionnaire template ID",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaireTemplateId}.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NoContent,
            Summary = "Deleted",
            Description = "All responses successfully deleted.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NotFound,
            Summary = "Not Found",
            Description = "Specified questionnaire template not found.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "v1/questionnaire-templates/{questionnaire-template-id}/responses")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var response = request.CreateResponse(HttpStatusCode.NoContent);
            await Task.CompletedTask;
            return response;
        }
    }
}
