namespace Application.DTOs.Evaluation
{
    public class UpdateResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public UpdateResponseDTO (bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
