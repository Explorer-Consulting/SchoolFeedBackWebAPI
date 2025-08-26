using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;


namespace AzureEndPointReaction.Functions.Questionnaires
{
    public sealed class SurveyGetRequestWorkerEncapsulator(ISurveyService service, ILogger<QuestionnaireEvaluationWorkerEncapsulator> logger)
    {
        private readonly ISurveyService _service = service;
        private readonly ILogger<QuestionnaireEvaluationWorkerEncapsulator> _logger = logger;

        [RequireStudent]
        [Function("PerformGetSurveys")]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(object) 
        )]
        public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys")] HttpRequestData request)
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

            var surveyDtoList = _service.GetSurveyMetadataForStudent(email);
            var ok = request.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(surveyDtoList);
            return ok;

        }
    }
}
