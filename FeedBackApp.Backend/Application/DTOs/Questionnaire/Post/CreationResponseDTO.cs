namespace Application.DTOs.Questionnaire.Post
{
    public class CreationResponseDTO
    {
        public bool Success { get; set; }

        public string Message { get; set; }
        
        public CreationResponseDTO(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
