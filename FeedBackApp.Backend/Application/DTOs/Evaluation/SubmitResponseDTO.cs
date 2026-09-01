
namespace Application.DTOs.Evaluation
{
    public class SubmitResponseDTO : BaseResponseDTO
    {
        public SubmitResponseDTO(bool success, string message) :
            base(success, message)
        { }
    }
}