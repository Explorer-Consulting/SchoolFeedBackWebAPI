using Application.DTOs.Evaluation;
using Application.Services.Interfaces;
using AzureFunctionsAPI.AzureEndPointReaction.Utils;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    /// <summary>
    /// HTTP endpoints for student questionnaire submission and update (Azure Functions – .NET isolated worker).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Orchestrates the lifecycle of a student's questionnaire by accepting new submissions and subsequent updates.
    /// Enforces authentication/authorization via a custom <c>[RequireStudent]</c> attribute and a per-request user context,
    /// then delegates business operations to <see cref="IEvaluationService"/>.
    /// </para>
    ///
    /// <para>
    /// <b>Authentication &amp; authorization</b><br/>
    /// The <c>[RequireStudent]</c> attribute ensures that only authenticated student identities can reach the handler.
    /// A <see cref="ClaimsPrincipal"/> is expected to be available in <c>request.FunctionContext.Items["User"]</c>.
    /// Ownership is enforced by comparing the caller's email (<c>ClaimTypes.NameIdentifier</c>) with the questionnaire id prefix
    /// (<c>id.Split('_')[0]</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Validation &amp; error handling</b><br/>
    /// Incoming JSON is parsed via <see cref="JsonUtil.ReadFromJsonAsync{T}(HttpRequestData)"/> and validated for null/empty payloads.
    /// Operational and domain errors are surfaced as appropriate HTTP statuses (<c>400</c>, <c>401</c>, <c>403</c>, <c>500</c>)
    /// with structured logging for diagnostics. Successful operations return <c>200 OK</c> with the service result.
    /// </para>
    ///
    /// <para>
    /// <b>OpenAPI metadata</b><br/>
    /// Attributes describe operation id, tags, path parameter <c>id</c>, request/response body contracts, and status codes
    /// for automatic Swagger generation in the Azure Functions OpenAPI extension.
    /// </para>
    /// </remarks>
    /// <param name="service">Domain service for questionnaire submission and updates.</param>
    /// <param name="logger">Structured logger for operational diagnostics.</param>
    public sealed class EvaluationFunctions(IEvaluationService service, ILogger<EvaluationFunctions> logger)
    {
        private readonly IEvaluationService _service = service;
        private readonly ILogger<EvaluationFunctions> _logger = logger;

        /// <summary>
        /// Submits a completed questionnaire for the current student.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Behavior</b><br/>
        /// Parses the incoming <see cref="SubmitQuestionnaireDTO"/>, verifies the presence of a student principal from the
        /// function context, checks ownership by comparing the student email with the questionnaire identifier prefix,
        /// then invokes <see cref="IEvaluationService.SubmitQuestionnaire(string, SubmitQuestionnaireDTO)"/>.
        /// </para>
        ///
        /// <para>
        /// <b>Responses</b><br/>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Submission accepted; returns <see cref="SubmitResponseDTO"/>.</description></item>
        ///   <item><description><c>400 Bad Request</c>: Missing/invalid JSON payload.</description></item>
        ///   <item><description><c>401 Unauthorized</c>: No user context present.</description></item>
        ///   <item><description><c>403 Unauthorized</c>: Ownership check failed for the questionnaire id.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Unhandled exception during processing.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request containing the JSON body.</param>
        /// <param name="id">Questionnaire identifier whose prefix must match the caller's email.</param>
        /// <returns>HTTP response including status and JSON body with the submission result.</returns>
        public async Task<HttpResponseData> PerformQuestionnaireSubmit(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "questionnaire/{id}")] HttpRequestData request,
            string id)
        {
            try
            {
                var dto = await JsonUtil.ReadFromJsonAsync<SubmitQuestionnaireDTO>(request);
                if (dto is null)
                {
                    _logger.LogError("Invalid or empty JSON body");
                    var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                    return badResponse;
                }

                var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;
                if (principal is null)
                {
                    var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync("Unauthorized: No user context found. Please log in.");
                    return unauthorizedResponse;
                }

                var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id.Split('_')[0] != email)
                {
                    var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync("Unauthorized: Questionnaire does not belong to the current user.");
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
                _logger.LogError("Something unexpected happenned! {Message}", e.Message);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new UpdateResponseDTO(false, $"Error submitting questionnaire: {e.Message}"));
                return response;
            }
        }

        /// <summary>
        /// Applies updates to an existing questionnaire for the current student.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Behavior</b><br/>
        /// Parses the incoming <see cref="UpdateQuestionnaireDTO"/>, verifies the student principal, performs the same
        /// ownership check against the questionnaire identifier, then calls
        /// <see cref="IEvaluationService.UpdateQuestionnaire(string, UpdateQuestionnaireDTO)"/>.
        /// </para>
        ///
        /// <para>
        /// <b>Responses</b><br/>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Update accepted; returns <see cref="UpdateResponseDTO"/>.</description></item>
        ///   <item><description><c>400 Bad Request</c>: Missing/invalid JSON payload or domain validation errors.</description></item>
        ///   <item><description><c>401 Unauthorized</c>: No user context present.</description></item>
        ///   <item><description><c>403 Unauthorized</c>: Ownership check failed for the questionnaire id.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Unhandled exception during processing.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request containing the JSON body.</param>
        /// <param name="id">Questionnaire identifier whose prefix must match the caller's email.</param>
        /// <returns>HTTP response including status and JSON body with the update result.</returns>
        //[Function("PerformQuestionnaireUpdate")]
        public async Task<HttpResponseData> PerformQuestionnaireUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "questionnaire/{id}")] HttpRequestData request,
            string id)
        {
            try
            {
                var dto = await JsonUtil.ReadFromJsonAsync<UpdateQuestionnaireDTO>(request);
                if (dto is null)
                {
                    _logger.LogError("Invalid or empty JSON body");
                    var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid or empty JSON body.");
                    return badResponse;
                }

                var principal = request.FunctionContext.Items["User"] as ClaimsPrincipal;
                if (principal is null)
                {
                    var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync("Unauthorized: No user context found. Please log in.");
                    return unauthorizedResponse;
                }

                var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id.Split('_')[0] != email)
                {
                    var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync("Unauthorized: Questionnaire does not belong to the current user.");
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
                _logger.LogError("Something unexpected happenned! {Message}", e.Message);
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new UpdateResponseDTO(false, $"Error updating questionnaire: {e.Message}"));
                return response;
            }
        }
    }
}
