namespace Application.DTOs.Questionnaire
{
    public class DeletionResponseDTO : BaseResponseDTO
    {
        public DeletionResponseDTO(bool success, string message)
            : base(success, message) { }
    }
}
