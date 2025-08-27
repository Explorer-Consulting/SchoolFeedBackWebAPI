using Application.DTOs.Questionnaire;

namespace Application.Services.Interfaces
{
    public interface IQuestionnaireService
    {
        public Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDTO dto);
        public Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id);
    }
}
