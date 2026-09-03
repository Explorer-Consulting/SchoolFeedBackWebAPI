namespace Application.DTOs.Questionnaire.Post
{
    public class ValidationResponseDTO : BaseResponseDTO
    {
        public ValidationResponseDTO(bool success, string message) : base(success, message) { }
    }
}
