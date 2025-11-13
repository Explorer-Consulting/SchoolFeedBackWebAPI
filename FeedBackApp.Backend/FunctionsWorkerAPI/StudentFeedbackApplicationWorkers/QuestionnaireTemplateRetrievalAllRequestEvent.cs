using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplateRetrievalAllRequestEvent
    {
        [Function(nameof(QuestionnaireTemplateRetrievalAllRequestEvent))]
        [OpenApiOperation(
            operationId: "ListQuestionnaireTemplates",
            tags: ["Questionnaire Templates"],
            Summary = "Retrieve all questionnaire templates",
            Description = "Returns a collection of all available questionnaire templates.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.OK,
            "application/json",
            typeof(object),
            Summary = "OK",
            Description = "List of questionnaire templates successfully retrieved.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NoContent,
            Summary = "No Content",
            Description = "No questionnaire templates available.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/questionnaire-templates")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("""
            [
              { "id": "template-001", "name": "Course Feedback" },
              { "id": "template-002", "name": "Event Evaluation" }
            ]
            """);
            return response;
        }
    }
}
