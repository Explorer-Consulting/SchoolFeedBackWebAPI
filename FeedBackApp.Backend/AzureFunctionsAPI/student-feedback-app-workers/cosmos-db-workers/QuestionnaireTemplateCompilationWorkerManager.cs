using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "questionnaire-template/compile")] HttpRequestData request)
        {
            logger.LogInformation("Logging questionnaire compilation");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("compilation of questionnaires");
            return ok;
        }
    }
}
