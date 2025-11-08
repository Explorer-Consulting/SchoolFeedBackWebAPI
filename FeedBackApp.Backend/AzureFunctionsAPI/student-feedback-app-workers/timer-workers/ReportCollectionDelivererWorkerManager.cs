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

namespace AzureFunctionsAPI.student_feedback_app_workers.timer_workers
{
    /// <summary>
    /// delivers all kinds of reports for specific users
    /// </summary>
    public sealed class ReportCollectionDelivererWorkerManager(ILogger<ReportCollectionDelivererWorkerManager> logger)
    {
        [Function(nameof(ReportCollectionDelivererWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates/{templateID}/reports:deliver")] HttpRequestData request, Guid templateID)
        {
            logger.LogInformation("logging delivering reports");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("delivering reports to specific users");
            return ok;
        }
    }
}
