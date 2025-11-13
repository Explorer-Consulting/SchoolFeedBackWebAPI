using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class ReportCompilationRequestEvent
    {
        [Function(nameof(ReportCompilationRequestEvent))]
        [OpenApiOperation(
            operationId: "Compile QuestionnaireReport",
            tags: ["Reports"],
            Summary = "Compile a questionnaire report",
            Description = "Generates or compiles a report based on the responses collected for a specific questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaire-template-id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Questionnaire template ID",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaire-template-id}.")]
        [OpenApiRequestBody(
            "application/json",
            typeof(object),
            Required = false,
            Description = "Optional parameters controlling report generation.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Accepted,
            "application/json",
            typeof(object),
            Summary = "Accepted",
            Description = "Report compilation request accepted.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Invalid input or missing parameters.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates/{questionnaire-template-id}/reports/compilation")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var response = request.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteStringAsync("Report compilation accepted.");
            return response;
        }
    }
}
