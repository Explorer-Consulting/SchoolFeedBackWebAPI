namespace Application.DTOs.Questionnaire.Post
{
    public class GenerateValidationTokenResponseDTO : BaseResponseDTO
    {
        public string? ValidationToken { get; set; }

        public GenerateValidationTokenResponseDTO(bool success, string message, string? validationToken = null):
            base(success, message)
        {
            ValidationToken = validationToken;
        }
    }
}
