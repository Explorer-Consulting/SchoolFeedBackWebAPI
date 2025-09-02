namespace Application.DTOs.Evaluation
{
    public class UpdateResponseDTO : BaseResponseDTO
    {
        public UpdateResponseDTO(bool success, string message)
            : base(success, message) { }
    }
}
