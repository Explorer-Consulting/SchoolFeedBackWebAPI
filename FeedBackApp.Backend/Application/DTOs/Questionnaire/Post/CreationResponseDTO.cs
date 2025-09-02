namespace Application.DTOs.Questionnaire
{
    public class CreationResponseDTO : BaseResponseDTO
    {
        public CreationResponseDTO(bool success, string message) :
            base(success, message)
        { }

    }
}
