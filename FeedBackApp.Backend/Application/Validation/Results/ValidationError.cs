namespace Application.Validation.Results
{
    /// <summary>
    /// Represents a single validation error with property name, error message, and optional error code.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// The name of the property that failed validation.
        /// May be empty for model-level validation errors.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message describing the validation failure.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Optional error code for programmatic handling of specific validation failures.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// The attempted value that failed validation (if applicable).
        /// </summary>
        public object? AttemptedValue { get; set; }
    }
}

