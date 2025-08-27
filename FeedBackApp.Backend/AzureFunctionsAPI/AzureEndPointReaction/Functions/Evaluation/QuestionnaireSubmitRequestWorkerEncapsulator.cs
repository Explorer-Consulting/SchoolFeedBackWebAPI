using Application.DTOs.Evaluation;
using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using SubmitQuestionnaireDTO = Application.DTOs.Evaluation.UpdateQuestionnaireDTO;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions.Evaluation;

public class QuestionnaireSubmitRequestWorkerEncapsulator(IEvaluationService service, ILogger<QuestionnaireSubmitRequestWorkerEncapsulator> logger)
{
    private readonly IEvaluationService _service = service;
    private readonly ILogger<QuestionnaireSubmitRequestWorkerEncapsulator> _logger = logger;

    [RequireStudent]
    [Function("PerformQuestionnaireSubmit")]
    [OpenApiOperation(
            operationId: "PerformQuestionnaireSubmit",
            tags: new[] { "Evaluations" }
        )]
    [OpenApiParameter(
            name: "id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string)
        )]
    [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(SubmitQuestionnaireDTO),
            Required = true
        )]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(UpdateResponseDTO)
        )]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}