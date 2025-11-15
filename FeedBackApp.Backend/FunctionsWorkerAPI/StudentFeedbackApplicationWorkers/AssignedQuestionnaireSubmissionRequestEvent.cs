using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class AssignedQuestionnaireSubmissionRequestEvent(ILogger<AssignedQuestionnaireSubmissionRequestEvent> logger)
    {
        [Function(nameof(AssignedQuestionnaireSubmissionRequestEvent))]
        [OpenApiOperation(
            operationId: "SubmitAssignedQuestionnaireResponse",
            tags: ["Assigned Questionnaires"],
            Summary = "Submit a subscriber's questionnaire response",
            Description = "Submits the final completed response for a subscriber assigned to a questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaire-template-id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Questionnaire template ID")]
        [OpenApiParameter(
            name: "subscriber-id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Subscriber ID")]
        [OpenApiParameter(
            name: "response-id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Response ID")]
        [OpenApiRequestBody(
            "application/json",
            typeof(object),
            Required = true,
            Description = "JSON body containing the completed response data.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Created,
            "application/json",
            typeof(object),
            Summary = "Created",
            Description = "Response successfully submitted.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Invalid or missing input data.")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "v1/questionnaire-templates/{questionnaire-template-id}/subscribers/{subscriber-id}/responses/{response-id}")]
            HttpRequestData request,
            Guid questionnaireTemplateId,
            Guid subscriberId,
            Guid responseId)
        {
            var response = request.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("Response successfully submitted.");
            return response;
        }
    }
}
