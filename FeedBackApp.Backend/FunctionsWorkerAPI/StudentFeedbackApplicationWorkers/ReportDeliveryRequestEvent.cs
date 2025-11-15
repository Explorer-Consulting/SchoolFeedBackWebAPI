using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class ReportDeliveryRequestEvent(ILogger<ReportDeliveryRequestEvent> logger)
    {
        [Function(nameof(ReportDeliveryRequestEvent))]
        [OpenApiOperation(
            operationId: "DeliverQuestionnaireReport",
            tags: ["Reports"],
            Summary = "Deliver a questionnaire report",
            Description = "Handles the delivery or distribution of a generated questionnaire report (e.g., via email or storage).")]
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
            Description = "Optional parameters defining the report delivery target or method.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Accepted,
            "application/json",
            typeof(object),
            Summary = "Accepted",
            Description = "Report delivery request accepted for processing.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Invalid or missing delivery parameters.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates/{questionnaire-template-id}/reports/delivery")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var response = request.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteStringAsync("Report delivery request accepted.");
            return response;
        }
    }
}
