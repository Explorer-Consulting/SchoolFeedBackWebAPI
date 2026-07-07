
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IQuestionnaireRepository
    {
        Task CompileAndSaveAsync(SurveyMetadata metadata);
        Task<bool> DeleteSurveyMetadataAsync(Guid id);
        Task<bool> DeleteQuestionnairesBySurveyIdAsync(Guid surveyId);
        Task<bool> DeleteQuestionTemplateBySurveyIdAsync(Guid surveyId);
        Task<List<SurveyMetadata>> GetSurveyMetadataForStudentAsync(string studentEmail);
        Task<SurveyMetadata?> GetSurveyMetadataAsync(Guid surveyId);
        Task<Questionnaire?> GetQuestionnaireByIdAsync(string id);
        Task<Questionnaire?> GetQuestionnaireByIdWithTrackingAsync(string id);
        Task<List<SurveyMetadata>> GetAllSurveyMetadata();
        Task UpdateQuestionnaireAsync(Questionnaire questionnaire);
    }
}
