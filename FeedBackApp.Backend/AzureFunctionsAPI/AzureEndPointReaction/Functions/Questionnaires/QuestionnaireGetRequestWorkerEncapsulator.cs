using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions.Questionnaires;

public class QuestionnaireGetRequestWorkerEncapsulator : IQuestionnaireWorker
{
    private readonly ILogger<QuestionnaireGetRequestWorkerEncapsulator> _logger;
    private readonly IQuestionnaireService _questionnaireService;

    public QuestionnaireGetRequestWorkerEncapsulator(ILogger<QuestionnaireGetRequestWorkerEncapsulator> logger, IQuestionnaireService questionnaireService)
    {
        _logger = logger;
        _questionnaireService = questionnaireService;
    }

    [RequireStudent]
    [Function("GetQuestionnaires")]
    public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questionnaires/{id}")] HttpRequestData request, Guid id)
    {
        var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;

        if (principal == null)
        {
            var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
            return unauthorizedResponse;
        }

        var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email))
        {
            var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Email not found in token.");
            return badResponse;
        }

        _logger.LogInformation("Student email: {Email}", email);

        var responseDto = await _questionnaireService.GetQuestionnairesAsync(id,email);

        if (responseDto == null)
        {
            var notFoundResponse = request.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Questionnaire with id {id} not found or not accessible for {email}.");
            return notFoundResponse;
        }
        var okResponse = request.CreateResponse(HttpStatusCode.OK);
        await okResponse.WriteAsJsonAsync(responseDto);
        return okResponse;
    }
}