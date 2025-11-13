using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplatePreviewRequestEvent
    {
        [Function(nameof(QuestionnaireTemplatePreviewRequestEvent))]
        [OpenApiOperation(
            operationId: "PreviewQuestionnaireTemplates",
            tags: ["QuestionnaireTemplates"],
            Summary = "Preview questionnaire templates",
            Description = "Returns a lightweight preview list of questionnaire templates.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.OK,
            "application/json",
            typeof(object),
            Summary = "OK",
            Description = "Preview list retrieved.")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/preview")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("""
            [
              { "id": "tmpl-001", "name": "Course Feedback (Preview)" },
              { "id": "tmpl-002", "name": "Event Evaluation (Preview)" }
            ]
            """);
            return response;
        }
    }
}
