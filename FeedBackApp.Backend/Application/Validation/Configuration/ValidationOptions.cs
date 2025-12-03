namespace Application.Validation.Configuration
{
    /// <summary>
    /// Configuration options for the validation layer.
    /// Allows customization of validation behavior across the application.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        /// Determines whether validation should throw exceptions on failure.
        /// If false, validation results are returned without throwing.
        /// Default: false
        /// </summary>
        public bool ThrowOnValidationFailure { get; set; } = false;

        /// <summary>
        /// Determines whether to include attempted values in validation error responses.
        /// Default: true
        /// </summary>
        public bool IncludeAttemptedValues { get; set; } = true;

        /// <summary>
        /// Determines whether to use error codes in validation responses.
        /// Default: true
        /// </summary>
        public bool UseErrorCodes { get; set; } = true;

        /// <summary>
        /// Custom error message prefix to apply to all validation errors.
        /// Default: null (no prefix)
        /// </summary>
        public string? ErrorMessagePrefix { get; set; }
    }
}

