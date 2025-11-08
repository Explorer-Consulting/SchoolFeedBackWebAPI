using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.student_feedback_app_workers.cosmos_db_workers;

/// <summary>
/// assigns a questionnaire template to specific users
/// </summary>
/// <param name="logger"></param>
public sealed class QuestionnaireTemplateAssignWorkerManager(ILogger<QuestionnaireTemplateAssignWorkerManager> logger)
{
    [Function(nameof(QuestionnaireTemplateAssignWorkerManager))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "questionnaire-templates/{templateID}/subscribers/{subscriberID}/responses")] HttpRequestData request, Guid templateID, Guid subscriberID)
    {
        logger.LogInformation("assigning to students");
        var ok = request.CreateResponse(System.Net.HttpStatusCode.Accepted);
        await ok.WriteStringAsync("Assigning to users");
        return ok;
    }
}
