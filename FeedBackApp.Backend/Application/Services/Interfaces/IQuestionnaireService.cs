
using Application.DTOs.GetQuestionnaires;
using Application.DTOs.Questionnaire;

namespace Application.Services.Interfaces
{
    public interface IQuestionnaireService
    {
        public Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDto dto);
        public Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id);
        public Task<GetQuestionnairesResponseDTO?> GetQuestionnairesAsync(Guid surveyId, string studentEmail);
    }
}
