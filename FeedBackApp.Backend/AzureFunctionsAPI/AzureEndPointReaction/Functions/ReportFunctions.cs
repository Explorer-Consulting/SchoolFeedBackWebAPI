using System.Net;
using Application.Exceptions;
using Application.Services;
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

        /// <summary>
        /// Compiles and queues email notifications for teachers and admins based on survey results.
        /// Extracts the survey ID from the question template parameter and triggers email compilation.
        /// </summary>
        /// <param name="request">HTTP request data.</param>
        /// <param name="questionTemplate">Question template identifier in format "prefix_surveyId".</param>
        /// <returns>HTTP response indicating the email delivery initiation status.</returns>
        [RequireAdmin]
        [Function("DeliverEvaluationReports")]
        [OpenApiOperation(
            operationId: "DeliverEvaluationReports",
            tags: ["Reports", "Email"],
            Summary = "Initiate email delivery for evaluation reports",
            Description = "Compiles and queues email notifications for teachers and administrators based on survey results.")]
        [OpenApiParameter(
            name: "questionTemplate",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Question template identifier (format: prefix_surveyId)",
            Description = "The template identifier from which the survey ID will be extracted.")]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.Accepted,
            contentType: "application/json",
            bodyType: typeof(object),
            Summary = "Email delivery initiated",
            Description = "Email compilation and queuing has been successfully initiated.")]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.BadRequest,
            contentType: "application/json",
            bodyType: typeof(object),
            Summary = "Invalid template format",
            Description = "The question template format is invalid or missing the survey ID.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden)]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.InternalServerError,
            contentType: "application/json",
            bodyType: typeof(object),
            Summary = "Email compilation failed",
            Description = "An error occurred while compiling email notifications.")]
        public async Task<HttpResponseData> DeliverEvaluationReports(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reports/send/{questionTemplate}")] 
            HttpRequestData request, 
            string questionTemplate)
        {
            if (string.IsNullOrWhiteSpace(questionTemplate))
            {
                _reportLogger.LogWarning("Empty questionTemplate parameter received in DeliverEvaluationReports");
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new 
                { 
                    error = "Invalid questionTemplate. The value cannot be null or whitespace.",
                    parameter = "questionTemplate"
                });
                return badRequest;
            }

            // Extract survey ID from template format: "prefix_surveyId"
            var templateParts = questionTemplate.Split('_', StringSplitOptions.RemoveEmptyEntries);
            
            if (templateParts.Length < 2)
            {
                _reportLogger.LogWarning(
                    "Invalid questionTemplate format received: {QuestionTemplate}. Expected format: prefix_surveyId", 
                    questionTemplate);
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new 
                { 
                    error = "Invalid questionTemplate format. Expected format: prefix_surveyId",
                    received = questionTemplate,
                    parameter = "questionTemplate"
                });
                return badRequest;
            }

            var surveyIdString = templateParts[1];

            if (string.IsNullOrWhiteSpace(surveyIdString) || !Guid.TryParse(surveyIdString, out var surveyId))
            {
                _reportLogger.LogWarning(
                    "Invalid survey ID extracted from questionTemplate: {QuestionTemplate}. Extracted: {SurveyId}", 
                    questionTemplate, 
                    surveyIdString);
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new 
                { 
                    error = "Invalid survey ID format. The survey ID must be a valid GUID.",
                    extractedSurveyId = surveyIdString,
                    parameter = "questionTemplate"
                });
                return badRequest;
            }

            _reportLogger.LogInformation(
                "DeliverEvaluationReports triggered. questionTemplate={QuestionTemplate}, surveyId={SurveyId}", 
                questionTemplate, 
                surveyId);

            try
            {
                await _emailService.CompileReportEmailsAsync(surveyId);

                _reportLogger.LogInformation(
                    "Successfully initiated email compilation for survey {SurveyId}", 
                    surveyId);

            var response = request.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new
            {
                    surveyId = surveyId.ToString(),
                    status = "Email delivery initiated",
                    message = "Email notifications have been queued for delivery to teachers and administrators."
            });
            return response;
            }
            catch (Exception ex)
            {
                _reportLogger.LogError(
                    ex, 
                    "Error while compiling report emails for survey {SurveyId}. Error: {ErrorMessage}", 
                    surveyId, 
                    ex.Message);

                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Failed to compile email notifications.",
                    surveyId = surveyId.ToString(),
                    message = "An error occurred while processing the email delivery request."
                });
                return errorResponse;
            }
        }
    }
}
