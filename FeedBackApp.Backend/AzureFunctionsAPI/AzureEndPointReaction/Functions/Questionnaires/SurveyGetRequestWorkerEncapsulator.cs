using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;


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
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Get successful" });
            return response;
        }
    }
}
