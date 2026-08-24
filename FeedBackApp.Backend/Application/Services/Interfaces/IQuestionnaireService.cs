using Application.DTOs.Questionnaire;
using Application.DTOs.Questionnaire.Post;
using Application.DTOs.Survey;

namespace Application.Services.Interfaces
{
    public interface IQuestionnaireService
    {
        public Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDTO dto);
        public Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id);
        public Task<QuestionnairesDTO> GetQuestionnairesAsync(Guid surveyId, string studentEmail);
        public Task<ValidationResponseDTO> ValidateQuestionnairesAsync(string validationToken);
        public Task<GenerateValidationTokenResponseDTO> GenerateValidationTokenAsync(Guid surveyId, string studentEmail);
    }
}
