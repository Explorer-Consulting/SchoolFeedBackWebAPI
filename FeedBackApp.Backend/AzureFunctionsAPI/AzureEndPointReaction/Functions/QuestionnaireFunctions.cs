using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
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

public sealed class QuestionnaireFunctions(IQuestionnaireService questionnaireService, ILogger<QuestionnaireFunctions> logger, IEmailService emailService, ISurveyService surveyService)
{
    private readonly IQuestionnaireService _questionnaireService = questionnaireService;
    private readonly ILogger<QuestionnaireFunctions> _logger = logger;
    private readonly IEmailService _emailService = emailService;
    private readonly ISurveyService _surveyService = surveyService;

    [RequireAdmin]
    [Function("PerformQuestionnaireCompilation")]
    [OpenApiOperation(
            operationId: "PerformQuestionnaireCompilation",
            tags: new[] { "Questionnaires" }
            )]
    [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(CreateSurveyMetadataDTO),
            Required = true
            )]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(CreationResponseDTO)
            )]
    public async Task<HttpResponseData> PerformQuestionnaireCompilation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "surveys")] HttpRequestData request)
    {
        try
        {
            var dto = await JsonUtil.ReadFromJsonAsync<CreateSurveyMetadataDTO>(request);

            if (dto == null)
            {
                _logger.LogError("Invalid or empty JSON body");
                var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                return badResponse;
            }

            var result = await _questionnaireService.CompileAndSaveAsync(dto);
            
            if (!result.Success)
            {
                var error = request.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteAsJsonAsync(result);
                return error;
            }

            var studentEmails = new List<string>();
            foreach (var set in dto.StudentSets)
            {
                foreach (var email in set.StudentEmails)
                {
                    studentEmails.Add(email);
                }
            }
            await _emailService.SendBulkEmailAsync(studentEmails, $"Tanár értékelés: {dto.Title}", $"Kérünk értékeld a tanáraid a következő kérdőíveken {dto.StartDate.ToShortDateString()}-től kezdődően: https://witty-beach-0b0c08903.2.azurestaticapps.net \nHatáridő: {dto.EndDate.Date.ToShortDateString()}");

            

            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;

        }
        catch (Exception e)
        {
            _logger.LogError("Something unexpected happenned!", e.Message);
            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new CreationResponseDTO(false, $"Error creating questionnaire: {e.Message}"));
            return response;
        }
    }

    [RequireAdmin]
    [Function("PerformQuestionnaireDeletion")]
    [OpenApiOperation(
            operationId: "PerformQuestionnaireDeletion",
            tags: new[] { "Questionnaires" }
        )]
    [OpenApiParameter(
            name: "id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(Guid)
        )]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(DeletionResponseDTO)
        )]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError)]
    public async Task<HttpResponseData> PerformQuestionnaireDeletion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "surveys/{id:guid}")] HttpRequestData request,
            Guid id)
    {
        try
        {

            DeletionResponseDTO result = await _questionnaireService.DeleteSurveyAsync(id);

            if (!result.Success)
            {
                var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteAsJsonAsync(result);
                return notFound;
            }

            var ok = request.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting questionnaire");

            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new DeletionResponseDTO
            (
                false,
                $"Error deleting questionnaire: {ex.Message}"
            ));
            return error;
        }
    }

    [RequireStudent]
    [Function("PerformGetQuestionnaires")]
    public async Task<HttpResponseData> PerformGetQuestionnaires([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "questionnaires/{id}")] HttpRequestData request, Guid id)
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

        var responseDto = await _questionnaireService.GetQuestionnairesAsync(id, email);

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

    [RequireAdmin]
    [Function("PerformGetSurveysAdmin")]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(object)
        )]
    public async Task<HttpResponseData> PerformGetSurveysAdmin([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys/admin")] HttpRequestData request)
    {
        var surveyDtoList = await _surveyService.GetAllSurveyMetadata();
        var ok = request.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(surveyDtoList);
        return ok;
    }

    [RequireStudent]
    [Function("PerformGetSurveys")]
    [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(object)
        )]
    public async Task<HttpResponseData> PerformGetSurveys([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys")] HttpRequestData request)
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

        var surveyDtoList = await _surveyService.GetSurveyMetadataForStudent(email);
        var ok = request.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(surveyDtoList);
        return ok;

    }
}