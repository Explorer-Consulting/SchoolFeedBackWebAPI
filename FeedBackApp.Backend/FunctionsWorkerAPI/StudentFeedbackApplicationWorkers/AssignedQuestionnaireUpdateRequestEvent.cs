using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class AssignedQuestionnaireUpdateRequestEvent(ILogger<AssignedQuestionnaireUpdateRequestEvent> logger)
    {
        [Function(nameof(AssignedQuestionnaireUpdateRequestEvent))]
        [OpenApiOperation(
            operationId: "UpdateSubscriberResponse",
            tags: ["Assigned Questionnaires"],
            Summary = "Update a subscriber response for a questionnaire template",
            Description = "Partially updates a subscriber's response for a questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaireTemplateId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string))]
        [OpenApiParameter(
            name: "subscriberId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string))]
        [OpenApiParameter(
            name: "responseId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string))]
        [OpenApiRequestBody("application/json", typeof(object), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Updated")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PATCH", Route = "v1/questionnaire-templates/{questionnaire-template-id}/subscribers/{subscriber-id}/responses/{response-id}")]
            HttpRequestData request,
            Guid questionnaireTemplateId,
            Guid subscriberId,
            Guid responseId)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Response updated.");
            return response;
        }
    }
}
