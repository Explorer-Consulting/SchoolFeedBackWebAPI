using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplateCompilationRequestEvent
    {
        [Function(nameof(QuestionnaireTemplateCompilationRequestEvent))]
        [OpenApiOperation(
            operationId: "CreateQuestionnaireTemplate",
            tags: ["Questionnaire Templates"],
            Summary = "requesting questionnaire template compilation",
            Description = "Questionnaire Template Compilation")]
        [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(object),
            Required = true,
            Description = "data needed for questionnaire template compilation in .json format")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Accepted,
            "application/json",
            typeof(object),
            Summary = "Accepted",
            Description = "the request is accepted, the compilation has started")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Missing or invalid data in the .json body")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates")] HttpRequestData request)
        {
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("accepted");
            return ok;
        }
    }
}
