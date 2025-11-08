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
    /// retrieve a specific questionnaire template
    /// </summary>
    public sealed class QuestionnaireTemplateRetrieveWorkerManager(ILogger<QuestionnaireTemplateRetrieveWorkerManager> logger)
    {
        [Function(nameof(QuestionnaireTemplateRetrieveWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "questionnaire-templates/{templateID}/retrieve")] HttpRequestData request, Guid templateID)
        {
            logger.LogInformation("logging retrieving a specific questionnaire template");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("retrieves a specific questionnaire template");
            return ok;
        }
    }
}
