using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsAPI.student_feedback_app_workers.cosmos_db_workers
{
    /// <summary>
    /// compiles a specific questionnaire template from source
    /// </summary>
    /// <param name="logger"></param>
    public sealed class QuestionnaireTemplateCompilationWorkerManager(ILogger<QuestionnaireTemplateCompilationWorkerManager> logger)
    {
        [Function(nameof(QuestionnaireTemplateCompilationWorkerManager))]
        [OpenApiOperation(
            operationId: "CompileQuestionnaireTemplate",
            tags: ["Questionnaire Template Compilation"],
            Summary = "Compiles a questionnaire template",
            Description = "Triggers a compilation process that builds a questionnaire template source into a deployable format."
        )]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-template:compile")] HttpRequestData request)
        {
            logger.LogInformation("Logging questionnaire compilation");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("compilation of questionnaires");
            return ok;
        }
    }
}
