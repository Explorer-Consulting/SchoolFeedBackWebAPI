using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsAPI.student_feedback_app_workers.blob_storage_workers
{
    /// <summary>
    /// retrieve all types of reports and zip them according to the current user
    /// </summary>
    /// <param name="logger"></param>

    public sealed class ReportRetrieveAllWorkerManager(ILogger<ReportRetrieveAllWorkerManager> logger)
    {
        // this will be a Blob trigger soon...
        [Function(nameof(ReportRetrieveAllWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "v1/questionnaire-templates/{templateID}/reports:retrieve-all")] HttpRequestData request, Guid templateID)
        {
            logger.LogInformation("I retrieve all types of reports");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("reports");
            return ok;
        }
    }
}
