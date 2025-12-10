namespace Application.DTOs
{
    public class ValidationErrorResponseDTO
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = "Validation failed";

        public IList<ValidationErrorDetail> Errors { get; set; } = new List<ValidationErrorDetail>();

        public string ErrorCode { get; set; } = "VALIDATION_ERROR";
    }

    public class ValidationErrorDetail
    {
        public string PropertyName { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public string? ErrorCode { get; set; }

        public object? AttemptedValue { get; set; }
    }
}

