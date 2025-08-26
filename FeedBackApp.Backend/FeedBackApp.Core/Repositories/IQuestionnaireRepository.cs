
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IQuestionnaireRepository
    {
        Task CompileAndSaveAsync(SurveyMetadata metadata);
        Task<bool> DeleteSurveyMetadataAsync(Guid id);
        Task<bool> DeleteQuestionnairesBySurveyIdAsync(Guid surveyId);
        Task<bool> DeleteQuestionTemplateBySurveyIdAsync(Guid surveyId);
        Task<bool> UpdateQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire);
        Task<Questionnaire> GetQuestionnaireByIdAsync(string id);
        Task<QuestionnaireTemplate> GetQuestionTemplateBySurveyIdAsync(string surevyId);
    }
}
