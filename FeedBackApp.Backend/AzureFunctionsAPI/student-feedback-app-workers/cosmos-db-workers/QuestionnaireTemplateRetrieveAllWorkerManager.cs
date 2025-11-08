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
    /// retrieves all questionnaire templates
    /// </summary>
    public sealed class QuestionnaireTemplateRetrieveAllWorkerManager(ILogger<QuestionnaireTemplateRetrieveAllWorkerManager> logger)
    {
        [Function(nameof(QuestionnaireTemplateRetrieveAllWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "questionnaire-templates/retrive-all")] HttpRequestData request)
        {
            logger.LogInformation("logging retriving tmeplates");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("retrieving all questionnaire templates");
            return ok;
        }
    }
}
