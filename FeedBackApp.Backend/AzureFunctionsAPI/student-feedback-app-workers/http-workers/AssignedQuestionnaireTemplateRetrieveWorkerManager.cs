using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsAPI.student_feedback_app_workers.http_workers
{
    /// <summary>
    /// retrieving a specific assigned questionnaire of a user
    /// </summary>
    /// <param name="logger"></param>
    public sealed class AssignedQuestionnaireTemplateRetrieveWorkerManager(ILogger<AssignedQuestionnaireTemplateRetrieveWorkerManager> logger)
    {
        [Function(nameof(AssignedQuestionnaireTemplateRetrieveWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/questionnaire-templates/{templateID}/subscribers/{subscriberID}/responses/{responseID}:retrieve")] HttpRequestData request, Guid templateID, Guid subscriberID, Guid responseID)
        {
            /*
             same validation logic as always
             */
            logger.LogInformation("logging assigned questionnaire of a user");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("retiureving an assigned questionnaire of a specific user");
            return ok;
        }
    }
}
