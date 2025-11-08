using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsAPI.student_feedback_app_workers.blob_storage_workers
{
    /// <summary>
    /// permits for a user to submit a response for a specific questionnaire template
    /// </summary>
    public sealed class AssignedQuestionnaireSubmissionWorkerManager(ILogger<AssignedQuestionnaireSubmissionWorkerManager> logger)
    {
        [Function(nameof(AssignedQuestionnaireSubmissionWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "questionnaire-templates/{templateID}/reports/compile")] HttpRequestData request, Guid templateID)
        {
            logger.LogInformation("logging submission of a specific user response to a specific questionnaire template");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("submitting a specific user answer for a specific questionnaire template");
            return ok;
        }
    }
}
