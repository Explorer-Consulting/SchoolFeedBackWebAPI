namespace Application.Validation.Results
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// Provides a standardized structure for validation success/failure and error messages.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Collection of validation errors. Empty if validation passed.
        /// </summary>
        public IList<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult
            {
                IsValid = true,
                Errors = new List<ValidationError>()
            };
        }

        /// <summary>
        /// Creates a failed validation result with the provided errors.
        /// </summary>
        /// <param name="errors">Collection of validation errors</param>
        public static ValidationResult Failure(IEnumerable<ValidationError> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        /// <summary>
        /// Creates a failed validation result with a single error.
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">Optional error code for programmatic handling</param>
        public static ValidationResult Failure(string propertyName, string errorMessage, string? errorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        PropertyName = propertyName,
                        ErrorMessage = errorMessage,
                        ErrorCode = errorCode
                    }
                }
            };
        }
    }
}

