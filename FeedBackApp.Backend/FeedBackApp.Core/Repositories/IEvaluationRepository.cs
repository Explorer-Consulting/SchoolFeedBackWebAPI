
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IEvaluationRepository
    {
        Task<bool> UpdateOrSubmitQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire);
        Task<Questionnaire?> GetQuestionnaireByIdAsync(string id);
        Task<Questionnaire?> GetQuestionnaresByIdAsNoTrackingAsync(string id);
        Task<QuestionnaireTemplate?> GetQuestionTemplateBySurveyIdAsync(string surevyId);
        Task<DateTime?> GetEndDateBySurveyId(string surveyId);

    }
}
