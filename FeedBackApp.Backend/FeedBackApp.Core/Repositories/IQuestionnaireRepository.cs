
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IQuestionnaireRepository
    {
        Task CompileAndSaveAsync(SurveyMetadata metadata);
        Task<bool> DeleteSurveyMetadataAsync(Guid id);
        Task<bool> DeleteQuestionnairesBySurveyIdAsync(Guid surveyId);
        Task<bool> DeleteQuestionTemplateBySurveyIdAsync(Guid surveyId);
        Task<SurveyMetadata?> GetSurveyMetadataAsync(Guid surveyId);
        Task<Questionnaire?> GetQuestionnaireByIdAsync(string id);
    }
}
