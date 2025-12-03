using Application.Validation.Results;

namespace Application.Validation.Interfaces
{
    /// <summary>
    /// Base interface for validation services that provides a standardized way
    /// to validate DTOs and return structured validation results.
    /// </summary>
    /// <typeparam name="T">The type of DTO to validate</typeparam>
    public interface IValidationService<T>
    {
        /// <summary>
        /// Validates the provided DTO instance and returns a validation result.
        /// </summary>
        /// <param name="dto">The DTO instance to validate</param>
        /// <returns>A validation result indicating success or failure with error details</returns>
        Task<ValidationResult> ValidateAsync(T dto);
    }
}

