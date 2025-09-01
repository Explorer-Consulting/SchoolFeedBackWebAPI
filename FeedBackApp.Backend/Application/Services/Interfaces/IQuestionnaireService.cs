using Application.DTOs.Questionnaire.GetQuestionnaires;
using Application.DTOs.Questionnaire.Post;
using Application.DTOs.Survey;

namespace Application.Services.Interfaces
{
    public interface IQuestionnaireService
    {
        public Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDto dto);
        public Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id);
        public Task<QuestionnairesDTO?> GetQuestionnairesAsync(Guid surveyId, string studentEmail);
    }
}
