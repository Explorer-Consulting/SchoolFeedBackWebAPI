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
    /// updates a specific assigned questionnaire of a user
    /// </summary>
    /// <param name="logger"></param>
    public sealed class AssignedQuestionnaireUpdateWorkerManager(ILogger<AssignedQuestionnaireUpdateWorkerManager> logger)
    {
        [Function(nameof(AssignedQuestionnaireUpdateWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PATCH", Route = "v1/questionnaire-templates/{templateID}/subscribers/{subscriberID}/responses/{responseID}:update")] HttpRequestData request, Guid templateID, Guid subscriberID, Guid responseID)
        {
            logger.LogInformation("updateing a specific assigned questionnaire");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("updating a specifi assignedd questionnaire for a specific user");
            return ok;
        }
    }
}
