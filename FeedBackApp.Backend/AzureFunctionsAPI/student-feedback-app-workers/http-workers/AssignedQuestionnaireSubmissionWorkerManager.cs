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
    /// permits for a user to submit a response for a specific questionnaire template
    /// </summary>

    public sealed class AssignedQuestionnaireSubmissionWorkerManager(ILogger<AssignedQuestionnaireSubmissionWorkerManager> logger)
    {
        [Function(nameof(AssignedQuestionnaireSubmissionWorkerManager))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PATCH", Route = "v1/questionnaire-templates/{templateID}/subscribers/{subscriberID}/responses/{responseID}:submit")] HttpRequestData request, Guid templateID, Guid subscriberID, Guid responseID)
        {
            // here come validators that validate id-formats and whether the users modifies their corresponding response or someone else
            /*
            1. template id:
                - if it's not Guid type (it's malformed) then: 400 Bad Request
                - if the Guid is not malformed, but doesn't correspond to any existing template: 404 Not Found
                - if the template exits but the current user is not authorized to access it: 403 Forbidden
            2. subscriber id:
                - ivalid id format: 400 Bad Request'
                - subscriber doesn't exist: 404 Not Found
                - if the subscriber exists, but the authenticated user is not allowed to view or act on behalf of that subscriber: 403 Forbidden
            3. response id:
                - invalid format: 400 Bad Request
                - if the responseId is valid in format but does not correspond to any existing response record in the database, return: 404 Not Found
                - if the response record exists but the authenticated user is not authorized to view, modify, or submit it (for example, another user’s response), return: 403 Forbidden
             */
            logger.LogInformation("logging submission of a specific user response to a specific questionnaire template");
            var ok = request.CreateResponse(HttpStatusCode.Accepted);
            await ok.WriteStringAsync("submitting a specific user answer for a specific questionnaire template");
            return ok;
        }
    }
}
