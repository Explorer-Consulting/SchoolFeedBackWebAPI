using Application.DTOs.Questionnaire;
using Application.DTOs.Questionnaire.Post;
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

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    /// <summary>
    /// HTTP endpoints for authoring, managing, and retrieving survey/questionnaire resources
    /// in the School Feedback application (Azure Functions – .NET isolated worker).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Exposes administrative operations (create/compile, delete, list) and student-facing
    /// read operations (list and fetch questionnaires eligible for a student). Business logic
    /// is delegated to <see cref="IQuestionnaireService"/> and <see cref="ISurveyService"/>.
    /// </para>
    ///
    /// <para>
    /// <b>Security model</b><br/>
    /// Access is enforced by custom authorization attributes:
    /// <list type="bullet">
    ///   <item><description><c>[RequireAdmin]</c>: only administrative identities may call the endpoint.</description></item>
    ///   <item><description><c>[RequireStudent]</c>: only authenticated student identities may call the endpoint.</description></item>
    /// </list>
    /// A <see cref="ClaimsPrincipal"/> is expected to be available in
    /// <c>HttpRequestData.FunctionContext.Items["User"]</c> (populated by upstream middleware).
    /// </para>
    ///
    /// <para>
    /// <b>OpenAPI</b><br/>
    /// Attributes on each operation describe operation id, tags, parameters, request/response
    /// contracts, and status codes for automatic Swagger generation (Azure Functions OpenAPI extension).
    /// </para>
    ///
    /// <para>
    /// <b>Error handling &amp; logging</b><br/>
    /// Endpoints validate inputs, return consistent HTTP status codes, and emit structured logs
    /// for visibility. Domain/service failures are surfaced as <c>400</c>/<c>404</c> as appropriate;
    /// unexpected faults return <c>500</c>.
    /// </para>
    /// </remarks>
    /// <param name="questionnaireService">Domain service for compiling, persisting, retrieving, and deleting questionnaires.</param>
    /// <param name="logger">Structured logger for operational diagnostics.</param>
    /// <param name="surveyService">Service exposing survey metadata queries for admin and student views.</param>
    public sealed class QuestionnaireFunctions(
        IQuestionnaireService questionnaireService,
        ILogger<QuestionnaireFunctions> logger,
        ISurveyService surveyService)
    {
        private readonly IQuestionnaireService _questionnaireService = questionnaireService;
        private readonly ILogger<QuestionnaireFunctions> _logger = logger;
        private readonly ISurveyService _surveyService = surveyService;

        /// <summary>
        /// Creates and persists a new survey by compiling its metadata and generating the associated questionnaires.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires administrator privileges (<c>[RequireAdmin]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b>: Parses <see cref="CreateSurveyMetadataDTO"/>, validates presence,
        /// and invokes <see cref="IQuestionnaireService.CompileAndSaveAsync(CreateSurveyMetadataDTO)"/>.
        /// Success returns a <see cref="CreationResponseDTO"/> with <c>200 OK</c>.
        /// Invalid payload yields <c>400 Bad Request</c>. Service-level failures are returned as <c>400 Bad Request</c>
        /// with the domain response, and unexpected exceptions as <c>500 Internal Server Error</c>.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request containing the survey metadata JSON body.</param>
        /// <returns>HTTP response encapsulating <see cref="CreationResponseDTO"/> or an error status.</returns>
        [RequireAdmin]
        [Function("PerformQuestionnaireCompilation")]
        [OpenApiOperation(operationId: "PerformQuestionnaireCompilation", tags: new[] { "Questionnaires" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateSurveyMetadataDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CreationResponseDTO))]
        public async Task<HttpResponseData> PerformQuestionnaireCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "surveys")] HttpRequestData request)
        {
            try
            {
                var dto = await JsonUtil.ReadFromJsonAsync<CreateSurveyMetadataDTO>(request);
                if (dto is null)
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

                var ok = request.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(result);
                return ok;
            }
            catch (Exception e)
            {
                _logger.LogError("Something unexpected happenned! {Message}", e.Message);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new CreationResponseDTO(false, $"Error creating questionnaire: {e.Message}"));
                return response;
            }
        }

        /// <summary>
        /// Validates an existing questionnaire by its identifier.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns> A HTTP response indicating the validation status. <see cref="ValidationResponseDTO"/> or an error status.</returns>
        [RequireAdmin]
        [Function("ValidateQuestionnaire")]
        [OpenApiOperation(operationId: "ValidateQuestionnaire", tags: new[] { "Questionnaires" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ID of the questionnaire to validate.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ValidationResponseDTO))]
        public async Task<HttpResponseData> ValidateQuestionnaire(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "surveys/{id}/validate")] HttpRequestData request, string id)
        {
            try
            {
                var result = await _questionnaireService.ValidateQuestionnaireAsync(id);
                if (!result.Success)
                {
                    var error = request.CreateResponse(HttpStatusCode.BadRequest);
                    await error.WriteAsJsonAsync(result);
                    return error;
                }

                var ok = request.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(result);
                return ok;
            } catch (Exception e)
            {
                _logger.LogError("Something unexpected happenned! {Message}", e.Message);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new ValidationResponseDTO(false, $"Error validating questionnaire: {e.Message}"));
                return response;
            }
        }

        /// <summary>
        /// Deletes an existing survey and its associated questionnaires by identifier.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires administrator privileges (<c>[RequireAdmin]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b>: Calls <see cref="IQuestionnaireService.DeleteSurveyAsync(Guid)"/>.
        /// On success, returns <c>200 OK</c> with <see cref="DeletionResponseDTO"/>. If the survey does not exist
        /// or cannot be removed according to domain rules, a <c>404 Not Found</c> is returned with the domain response.
        /// Unexpected faults result in <c>500 Internal Server Error</c>.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required).</param>
        /// <param name="id">Survey identifier to delete.</param>
        /// <returns>HTTP response encapsulating <see cref="DeletionResponseDTO"/> or an error status.</returns>
        [RequireAdmin]
        [Function("PerformQuestionnaireDeletion")]
        [OpenApiOperation(operationId: "PerformQuestionnaireDeletion", tags: new[] { "Questionnaires" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeletionResponseDTO))]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseData> PerformQuestionnaireDeletion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "surveys/{id:guid}")] HttpRequestData request,
            Guid id)
        {
            try
            {
                var result = await _questionnaireService.DeleteSurveyAsync(id);
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
                await error.WriteAsJsonAsync(new DeletionResponseDTO(false, $"Error deleting questionnaire: {ex.Message}"));
                return error;
            }
        }

        /// <summary>
        /// Retrieves a student-specific questionnaire set for a given survey identifier.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires a student identity (<c>[RequireStudent]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b>: Extracts the student principal from the function context, validates that an email
        /// (<see cref="ClaimTypes.NameIdentifier"/>) is present, then invokes
        /// <see cref="IQuestionnaireService.GetQuestionnairesAsync(Guid, string)"/> to fetch student-scoped data.
        /// Missing user context returns <c>401 Unauthorized</c>, missing email returns <c>400 Bad Request</c>.
        /// If no data is available for the user, returns <c>404 Not Found</c>. Success returns <c>200 OK</c> with the payload.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required).</param>
        /// <param name="id">Survey identifier to scope the questionnaires.</param>
        /// <returns>HTTP response with the student-scoped questionnaire data or an error status.</returns>
        [RequireStudent]
        [Function("PerformGetSurveyData")]
        public async Task<HttpResponseData> PerformGetSurveyData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys/{id}")] HttpRequestData request,
            Guid id)
        {
            var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;
            if (principal is null)
            {
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: No user context found. Please log in.");
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
            if (responseDto is null)
            {
                var notFoundResponse = request.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Questionnaire with id {id} not found or not accessible for {email}.");
                return notFoundResponse;
            }

            var okResponse = request.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(responseDto);
            return okResponse;
        }

        /// <summary>
        /// Retrieves the complete survey metadata list for administrators.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires administrator privileges (<c>[RequireAdmin]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b>: Calls <see cref="ISurveyService.GetAllSurveyMetadata"/> and returns the results with <c>200 OK</c>.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required).</param>
        /// <returns>HTTP response containing the survey metadata collection.</returns>
        [RequireAdmin]
        [Function("PerformGetSurveysAdmin")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object))]
        public async Task<HttpResponseData> PerformGetSurveysAdmin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "management/surveys")] HttpRequestData request)
        {
            var surveyDtoList = await _surveyService.GetAllSurveyMetadata();
            var ok = request.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(surveyDtoList);
            return ok;
        }

        /// <summary>
        /// Retrieves the list of survey metadata available to the current student.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires a student identity (<c>[RequireStudent]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b>: Resolves the student's email from the request context and calls
        /// <see cref="ISurveyService.GetSurveyMetadataForStudent(string)"/>. Missing user context returns
        /// <c>401 Unauthorized</c>, missing email returns <c>400 Bad Request</c>. Success returns <c>200 OK</c>
        /// with the metadata list.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required).</param>
        /// <returns>HTTP response containing the student-scoped survey metadata collection.</returns>
        [RequireStudent]
        [Function("PerformGetSurveys")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object))]
        public async Task<HttpResponseData> PerformGetSurveys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys")] HttpRequestData request)
        {
            var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;
            if (principal is null)
            {
                var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: No user context found. Please log in.");
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
}
