using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions.Questionnaires;

public class SurveyAdminGetRequestWorkerEncapsulator
{
    private readonly ILogger<SurveyAdminGetRequestWorkerEncapsulator> _logger;
    private readonly ISurveyService _service;
    public SurveyAdminGetRequestWorkerEncapsulator(ILogger<SurveyAdminGetRequestWorkerEncapsulator> logger, ISurveyService surveyService)
    {
        _logger = logger;
        _service = surveyService;
    }

    [RequireAdmin]
    [Function("PerformGetSurveysAdmin")]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(object)
        )]
    public async Task<HttpResponseData> ExecuteTaskAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys/admin")] HttpRequestData request)
    {

        var surveyDtoList = await _service.GetAllSurveyMetadata();
        var ok = request.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(surveyDtoList);
        return ok;

    }
}