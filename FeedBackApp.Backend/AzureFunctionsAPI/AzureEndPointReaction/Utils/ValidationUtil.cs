using Application.DTOs;
using Application.Validation.Results;
using FluentValidation;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace AzureFunctionsAPI.AzureEndPointReaction.Utils
{
    public static class ValidationUtil
    {
        public static async Task<(T? Dto, HttpResponseData? ErrorResponse)> ReadAndValidateAsync<T>(
            HttpRequestData request,
            IValidator<T> validator,
            ILogger? logger = null)
        {
            // Read DTO from request body
            var dto = await JsonUtil.ReadFromJsonAsync<T>(request);

            if (dto == null)
            {
                logger?.LogWarning("Failed to deserialize request body to {Type}", typeof(T).Name);
                var errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                var validationError = new ValidationErrorResponseDTO
                {
                    Success = false,
                    Message = "Invalid or empty request body",
                    ErrorCode = "INVALID_REQUEST_BODY",
                    Errors = new List<ValidationErrorDetail>
                    {
                        new ValidationErrorDetail
                        {
                            PropertyName = "Body",
                            ErrorMessage = "Request body is missing, empty, or cannot be deserialized",
                            ErrorCode = "INVALID_JSON"
                        }
                    }
                };
                await errorResponse.WriteAsJsonAsync(validationError);
                return (default, errorResponse);
            }

            // Validate the DTO
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                logger?.LogWarning("Validation failed for {Type}: {ErrorCount} errors", typeof(T).Name, validationResult.Errors.Count);
                
                var errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                var validationError = ConvertToErrorResponse(validationResult);
                await errorResponse.WriteAsJsonAsync(validationError);
                return (default, errorResponse);
            }

            // Validation passed
            return (dto, null);
        }

        public static ValidationErrorResponseDTO ConvertToErrorResponse(
            FluentValidation.Results.ValidationResult validationResult)
        {
            var errors = validationResult.Errors.Select(error => new ValidationErrorDetail
            {
                PropertyName = error.PropertyName,
                ErrorMessage = error.ErrorMessage,
                ErrorCode = error.ErrorCode,
                AttemptedValue = error.AttemptedValue
            }).ToList();

            return new ValidationErrorResponseDTO
            {
                Success = false,
                Message = $"Validation failed with {errors.Count} error(s)",
                ErrorCode = "VALIDATION_ERROR",
                Errors = errors
            };
        }
        public static async Task<HttpResponseData> CreateValidationErrorResponseAsync(
            HttpRequestData request,
            FluentValidation.Results.ValidationResult validationResult,
            ILogger? logger = null)
        {
            logger?.LogWarning("Creating validation error response with {ErrorCount} errors", validationResult.Errors.Count);
            
            var errorResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            var validationError = ConvertToErrorResponse(validationResult);
            await errorResponse.WriteAsJsonAsync(validationError);
            return errorResponse;
        }
    }
}

