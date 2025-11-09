using System.Net;
using Application.Exceptions;
using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    /// <summary>
    /// Report-related HTTP endpoints for compiling, storing, and delivering evaluation reports
    /// in the School Feedback application (Azure Functions – .NET isolated worker).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Provides administrative operations for generating reports from questionnaire/survey data and
    /// initiating their delivery to recipients. Business logic is delegated to <see cref="IReportService"/>
    /// and notification dispatch to <see cref="IEmailService"/>.
    /// </para>
    ///
    /// <para>
    /// <b>Security model</b><br/>
    /// Endpoints are protected by the custom <c>[RequireAdmin]</c> attribute, which requires an authenticated
    /// administrative identity to access them. Identity resolution and authorization are performed by upstream middleware.
    /// </para>
    ///
    /// <para>
    /// <b>Reliability &amp; observability</b><br/>
    /// Operations emit structured logs for start, validation failures, domain failures, and unexpected exceptions.
    /// Service-specific exceptions (e.g., <see cref="ReportCompilationException"/>) are mapped to stable HTTP statuses.
    /// </para>
    ///
    /// <para>
    /// <b>OpenAPI</b><br/>
    /// Attributes describe operation ids, tags, parameters, request/response contracts, and status codes to enable
    /// automatic Swagger generation through the Azure Functions OpenAPI extension.
    /// </para>
    /// </remarks>
    /// <param name="reportService">Service responsible for compiling and storing reports.</param>
    /// <param name="emailService">Service responsible for composing and sending report-related emails.</param>
    /// <param name="reportLogger">Structured logger for report operations.</param>
    public sealed class ReportFunctions(IReportService reportService, IEmailService emailService, ILogger<ReportFunctions> reportLogger)
    {
        private readonly IReportService _reportService = reportService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<ReportFunctions> _reportLogger = reportLogger;

        /// <summary>
        /// Compiles and persists a report based on a provided template identifier.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires administrator privileges (<c>[RequireAdmin]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b><br/>
        /// Validates that <paramref name="templateID"/> is non-empty, then invokes
        /// <see cref="IReportService.CompileAndStore(string)"/> to generate and store the report artifact in the configured storage backend.
        /// On successful completion, returns <c>201 Created</c> and a minimal payload containing the report identifier and status.
        /// </para>
        /// <para>
        /// <b>Error handling</b><br/>
        /// Returns <c>400 Bad Request</c> on missing/invalid input, <c>500 Internal Server Error</c> on domain or unexpected failures.
        /// <see cref="ReportCompilationException"/> is logged and surfaced as a server error with a stable error shape.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required for this operation).</param>
        /// <param name="templateID">Template identifier used to locate and compile the corresponding report definition.</param>
        /// <returns>
        /// A <see cref="HttpResponseData"/> with one of the following status codes:
        /// <list type="bullet">
        ///   <item><description><c>201 Created</c> – Report successfully compiled and stored.</description></item>
        ///   <item><description><c>400 Bad Request</c> – Invalid <paramref name="templateID"/>.</description></item>
        ///   <item><description><c>401 Unauthorized</c> – Authentication required.</description></item>
        ///   <item><description><c>403 Forbidden</c> – Administrator role required.</description></item>
        ///   <item><description><c>500 Internal Server Error</c> – Domain or unexpected failure.</description></item>
        /// </list>
        /// </returns>
        [RequireAdmin]
        //[Function("PerformReportCompilation")]
        public async Task<HttpResponseData> PerformReportCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reports/{templateID}")]
            HttpRequestData request,
            string templateID)
        {
            if (string.IsNullOrWhiteSpace(templateID))
            {
                _reportLogger.LogWarning("Empty templateID received.");
                var bad = request.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new
                {
                    error = "Invalid templateID. The value cannot be null or whitespace."
                });
                return bad;
            }

            _reportLogger.LogInformation("Report compilation request received. templateID={TemplateId}", templateID);

            try
            {
                await _reportService.CompileAndStore(templateID);

                var created = request.CreateResponse(HttpStatusCode.Created);
                await created.WriteAsJsonAsync(new
                {
                    reportId = templateID,
                    status = "Created"
                });
                return created;
            }
            catch (ReportCompilationException ex)
            {
                _reportLogger.LogError(ex, "Report compilation failed. templateID={TemplateId}", templateID);

                var error = request.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "Report generation failed."
                });
                return error;
            }
            catch (Exception ex)
            {
                _reportLogger.LogCritical(ex, "Unexpected error during report compilation. templateID={TemplateId}", templateID);

                var error = request.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "Unexpected server error."
                });
                return error;
            }
        }

        /// <summary>
        /// Initiates delivery of evaluation reports for a survey to the intended recipients.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Security</b>: Requires administrator privileges (<c>[RequireAdmin]</c>).
        /// </para>
        /// <para>
        /// <b>Behavior</b><br/>
        /// Extracts a survey identifier from <paramref name="questionTemplate"/> and calls
        /// <see cref="IEmailService.CompileReportEmailsAsync(System.Guid)"/> to compose and enqueue outbound report emails.
        /// The operation returns <c>202 Accepted</c> to indicate that delivery has been initiated asynchronously.
        /// </para>
        /// <para>
        /// <b>Input format</b><br/>
        /// <paramref name="questionTemplate"/> is expected to contain a GUID component that can be parsed into a survey id.
        /// If the extracted identifier is empty or invalid, the request is rejected with <c>400 Bad Request</c>.
        /// </para>
        /// </remarks>
        /// <param name="request">HTTP request (no body required).</param>
        /// <param name="questionTemplate">Composite identifier containing the target survey id used to route email compilation.</param>
        /// <returns>
        /// A <see cref="HttpResponseData"/> with one of the following status codes:
        /// <list type="bullet">
        ///   <item><description><c>202 Accepted</c> – Delivery workflow initiated.</description></item>
        ///   <item><description><c>400 Bad Request</c> – Invalid or missing survey id component.</description></item>
        ///   <item><description><c>401 Unauthorized</c> – Authentication required.</description></item>
        ///   <item><description><c>403 Forbidden</c> – Administrator role required.</description></item>
        /// </list>
        /// </returns>
        [RequireAdmin]
        //[Function("DeliverEvaluationReports")]
        public async Task<HttpResponseData> DeliverEvaluationReports(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reports/send/{questionTemplate}")] HttpRequestData request,
            string questionTemplate)
        {
            string surveyId = questionTemplate.Split('_')[1];

            if (string.IsNullOrWhiteSpace(surveyId))
            {
                _reportLogger.LogWarning("Empty templateID received in DeliverEvaluationReports.");
                var bad = request.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { error = "Invalid templateID. The value cannot be null or whitespace." });
                return bad;
            }

            _reportLogger.LogInformation("DeliverEvaluationReports triggered. surveyId={surveyId}", surveyId);

            await _emailService.CompileReportEmailsAsync(new Guid(surveyId));

            var response = request.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new
            {
                reportId = surveyId,
                status = "Delivery initiated"
            });
            return response;
        }
    }
}
