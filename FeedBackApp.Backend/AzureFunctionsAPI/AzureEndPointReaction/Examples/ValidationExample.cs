using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Services.Interfaces;
using AzureFunctionsAPI.AzureEndPointReaction.Utils;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using FluentValidation;

namespace AzureFunctionsAPI.AzureEndPointReaction.Examples
{
    /// <summary>
    /// Example Azure Function demonstrating how to use the validation pipeline.
    /// This shows the recommended pattern for validating DTOs before processing.
    /// </summary>
    public class ValidationExample
    {
        private readonly IValidator<CreateSurveyMetadataDTO> _validator;
        private readonly IQuestionnaireService _questionnaireService;
        private readonly ILogger<ValidationExample> _logger;

        public ValidationExample(
            IValidator<CreateSurveyMetadataDTO> validator,
            IQuestionnaireService questionnaireService,
            ILogger<ValidationExample> logger)
        {
            _validator = validator;
            _questionnaireService = questionnaireService;
            _logger = logger;
        }

        /// <summary>
        /// Example function showing the validation pipeline pattern.
        /// 
        /// Key points:
        /// 1. Validator is injected via constructor (automatically registered)
        /// 2. ValidationUtil.ReadAndValidateAsync is used to read and validate DTO
        /// 3. If validation fails, error response is returned immediately
        /// 4. If validation passes, DTO is guaranteed valid for business logic
        /// </summary>
        [RequireAdmin]
        [Function("ExampleWithValidation")]
        [OpenApiOperation(
            operationId: "ExampleWithValidation",
            tags: new[] { "Examples" })]
        [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(CreateSurveyMetadataDTO),
            Required = true)]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.OK,
            contentType: "application/json",
            bodyType: typeof(CreationResponseDTO))]
        [OpenApiResponseWithBody(
            statusCode: HttpStatusCode.BadRequest,
            contentType: "application/json",
            bodyType: typeof(Application.DTOs.ValidationErrorResponseDTO))]
        public async Task<HttpResponseData> ExampleWithValidation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "example/surveys")] 
            HttpRequestData request)
        {
            // STEP 1: Read and validate DTO using ValidationUtil
            // This handles:
            // - Deserialization from request body
            // - Null checking
            // - Validation using FluentValidation
            // - Standardized error response creation
            var (dto, validationError) = await ValidationUtil.ReadAndValidateAsync<CreateSurveyMetadataDTO>(
                request,
                _validator,
                _logger);

            // STEP 2: Handle validation failure
            // If validation failed, return the standardized error response immediately
            // The error response contains:
            // - success: false
            // - message: Summary message
            // - errorCode: "VALIDATION_ERROR"
            // - errors: Array of individual validation errors
            if (validationError != null)
            {
                _logger.LogWarning("Validation failed for CreateSurveyMetadataDTO");
                return validationError;
            }

            // STEP 3: Process validated DTO
            // At this point, dto is guaranteed to be:
            // - Not null
            // - Valid according to all validation rules
            // - Safe to use in business logic without additional validation checks
            try
            {
                // dto is guaranteed to be non-null after validation passes
                var result = await _questionnaireService.CompileAndSaveAsync(dto!);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing validated DTO");
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new CreationResponseDTO(
                    false,
                    $"Error processing request: {ex.Message}"));
                return errorResponse;
            }
        }
    }
}

