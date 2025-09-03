using Application.DTOs.Evaluation;
using Application.Services.Interfaces;
using AzureFunctionsAPI.AzureEndPointReaction.Utils;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Security.Claims;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions;

public sealed class EvaluationFunctions(IEvaluationService service, ILogger<EvaluationFunctions> logger)
{
    private readonly IEvaluationService _service = service;
    private readonly ILogger<EvaluationFunctions> _logger = logger;

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
            bodyType: typeof(SubmitResponseDTO)
        )]
    public async Task<HttpResponseData> PerformQuestionnaireSubmit([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questionnaire/{id}")] HttpRequestData request, string id)
    {
        try
        {
            var dto = await JsonUtil.ReadFromJsonAsync<SubmitQuestionnaireDTO>(request);

            if (dto == null)
            {
                _logger.LogError("Invalid or empty JSON body");
                var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                return badResponse;
            }

            var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;

            if (principal == null)
            {
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorizedResponse;
            }

            var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.Split('_')[0] != email){
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorizedResponse;
            }

            var result = await _service.SubmitQuestionnaire(id, dto);

            if (!result.Success)
            {
                var error = request.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteAsJsonAsync(result);
                return error;
            }

            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError("Something unexpected happenned!", e.Message);
            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new UpdateResponseDTO(false, $"Error submitting questionnaire: {e.Message}"));
            return response;
        }

    }
    [RequireStudent]
    [Function("PerformQuestionnaireUpdate")]
    [OpenApiOperation(
            operationId: "PerformQuestionnaireUpdate",
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
            bodyType: typeof(UpdateQuestionnaireDTO),
            Required = true
        )]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(UpdateResponseDTO)
        )]
    public async Task<HttpResponseData> PerformQuestionnaireUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "questionnaire/{id}")] HttpRequestData request, string id)
    {
        try
        {
            var dto = await JsonUtil.ReadFromJsonAsync<UpdateQuestionnaireDTO>(request);

            if (dto == null)
            {
                _logger.LogError("Invalid or empty JSON body");
                var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                return badResponse;
            }

            var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;

            if (principal == null)
            {
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorizedResponse;
            }

            var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id.Split('_')[0] != email)
            {
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorizedResponse;
            }

            var result = await _service.UpdateQuestionnaire(id, dto);

            if (!result.Success)
            {
                var error = request.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteAsJsonAsync(result);
                return error;
            }

            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError("Something unexpected happenned!", e.Message);
            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new UpdateResponseDTO(false, $"Error updating questionnaire: {e.Message}"));
            return response;
        }

    }
}