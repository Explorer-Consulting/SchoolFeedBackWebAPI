using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsAPI.student_feedback_app_workers.http_workers
{
    /// <summary>
    /// retrieves all of the assigned questionnaire responses for a specific user
    /// </summary>
    public sealed class AssignedQuestionnaireTemplateRetrieveAllWorkerManager(ILogger<AssignedQuestionnaireTemplateRetrieveAllWorkerManager> logger)
    {
        [Function(nameof(AssignedQuestionnaireTemplateRetrieveAllWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/questionnaire-templates/{templateID}/subscribers/{subscriberID}/responses:retrieve-all")] HttpRequestData request, Guid templateID, Guid subscriberID)
        {
            /*
             validations are mostly the same as at the other functions
             */
            logger.LogInformation("logging retrieving all of the assigned tmeplates for a specific user");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("retireving all assigned templates for a specific usetr");
            return ok;
        }
    }
}
