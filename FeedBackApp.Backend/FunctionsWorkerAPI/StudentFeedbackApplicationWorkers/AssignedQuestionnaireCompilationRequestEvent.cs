using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class AssignedQuestionnaireCompilationRequestEvent(ILogger<AssignedQuestionnaireCompilationRequestEvent> logger)
    {
        [Function(nameof(AssignedQuestionnaireCompilationRequestEvent))]
        [OpenApiOperation(
            operationId: "CompileAssignedQuestionnaireResponse",
            tags: ["Assigned Questionnaires"],
            Summary = "Compile a subscriber's questionnaire response",
            Description = "Creates or compiles a new response for a subscriber assigned to a specific questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaireTemplateId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Questionnaire template ID",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaireTemplateId}.")]
        [OpenApiParameter(
            name: "subscriberId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Subscriber ID",
            Description = "Path parameter of .../subscribers/{subscriberId}.")]
        [OpenApiRequestBody(
            "application/json",
            typeof(object),
            Required = true,
            Description = "JSON body containing the subscriber's response data.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Accepted,
            "application/json",
            typeof(object),
            Summary = "Accepted",
            Description = "Response compilation accepted for processing.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Invalid or missing input data.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates/{questionnaire-template-id}/subscribers/{subscriber-id}/responses")]
            HttpRequestData request,
            Guid questionnaireTemplateId,
            Guid subscriberId)
        {
            var response = request.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteStringAsync("Response compilation accepted.");
            return response;
        }
    }
}
