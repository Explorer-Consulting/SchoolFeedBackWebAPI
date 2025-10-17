using System.Net;
using Application.Exceptions;
using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    public sealed class ReportFunctions(IReportService reportService,IEmailService emailService, ILogger<ReportFunctions> reportLogger)
    {
        private readonly IReportService _reportService = reportService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<ReportFunctions> _reportLogger = reportLogger;

        [RequireAdmin]
        [Function("PerformReportCompilation")]
        [OpenApiOperation(
            operationId: "PerformReportCompilation",
            tags: ["Reports"],
            Summary = "Compile and store a report",
            Description = "Triggers report compilation and stores the result in the configured storage.")]
        [OpenApiParameter(
            name: "templateID",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Template identifier (string or GUID depending on business logic).")]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.Created,
            contentType: "application/json",
            bodyType: typeof(object),
            Summary = "Report successfully created",
            Description = "Returns the report identifier and status.")]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.InternalServerError,
            contentType: "application/json",
            bodyType: typeof(object),
            Summary = "Report generation failed",
            Description = "Unexpected error during report compilation.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Summary = "Invalid template ID format")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden)]
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

        // ezt kellene implementalni, vagyis kellene meg egy service a BLOB-oknak
        [RequireAdmin]
        [Function("DeliverEvaluationReports")]
        public async Task<HttpResponseData> DeliverEvaluationReports(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reports/send/{questionTemplate}")] HttpRequestData request, string questionTemplate)
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
