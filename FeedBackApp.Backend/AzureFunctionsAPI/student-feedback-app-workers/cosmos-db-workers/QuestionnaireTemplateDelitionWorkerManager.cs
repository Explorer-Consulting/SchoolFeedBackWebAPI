using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsAPI.student_feedback_app_workers.cosmos_db_workers
{
    /// <summary>
    /// deleting a specific questionnaire template
    /// </summary>
    /// <param name="logger"></param>
    public sealed class QuestionnaireTemplateDelitionWorkerManager(ILogger<QuestionnaireTemplateDelitionWorkerManager> logger)
    {
        [Function(nameof(QuestionnaireTemplateDelitionWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "v1/questionnaire-templates/{templateID}:delete")] HttpRequestData request, Guid templateID)
        {
            logger.LogInformation("logging  questionnaire template deletion");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("deleting a given questionnaire template");
            return ok;
        }
    }
}
