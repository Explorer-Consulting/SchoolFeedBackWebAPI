using Application.Validation.Results;
using FluentValidation;

namespace Application.Validation.Base
{
    /// <summary>
    /// Base abstract class for all validators in the application.
    /// Provides common functionality and ensures consistent validation behavior.
    /// </summary>
    /// <typeparam name="T">The type of DTO to validate</typeparam>
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Converts FluentValidation's ValidationResult to our standardized ValidationResult.
        /// </summary>
        /// <param name="fluentValidationResult">The result from FluentValidation</param>
        /// <returns>A standardized ValidationResult</returns>
        protected ValidationResult ToValidationResult(FluentValidation.Results.ValidationResult fluentValidationResult)
        {
            if (fluentValidationResult.IsValid)
            {
                return ValidationResult.Success();
            }

            var errors = fluentValidationResult.Errors.Select(error => new ValidationError
            {
                PropertyName = error.PropertyName,
                ErrorMessage = error.ErrorMessage,
                ErrorCode = error.ErrorCode,
                AttemptedValue = error.AttemptedValue
            });

            return ValidationResult.Failure(errors);
        }
    }
}

