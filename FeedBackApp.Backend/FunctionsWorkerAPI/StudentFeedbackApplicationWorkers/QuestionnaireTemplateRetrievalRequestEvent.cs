using System.Net;
using ApplicationLayer.Attributes;
using ApplicationLayer.DataTransferObjects;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplateRetrievalRequestEvent(ILogger<QuestionnaireTemplateRetrievalRequestEvent> logger)
    {
        [Function(nameof(QuestionnaireTemplateRetrievalRequestEvent))]
        [OpenApiOperation(
            operationId: "GetQuestionnaireTemplate",
            tags: ["Questionnaire Templates"],
            Summary = "Retrieve a questionnaire template",
            Description = "Retrieves the details of a questionnaire template identified by its unique ID.")]
        [OpenApiParameter(
            name: "questionnaireTemplateId",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Unique identifier of the questionnaire template",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaire-template-id}.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.OK,
            "application/json",
            typeof(object),
            Summary = "OK",
            Description = "Questionnaire template successfully retrieved.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.NotFound,
            Summary = "Not Found",
            Description = "The specified questionnaire template was not found.")]

        [ValidateRequest(typeof(QuestionnaireTemplateDTO))]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/questionnaire-templates/{questionnaire-template-id}")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"{{ \"id\": \"{questionnaireTemplateId}\", \"name\": \"Example Questionnaire Template\" }}");
            return response;
        }
    }
}
