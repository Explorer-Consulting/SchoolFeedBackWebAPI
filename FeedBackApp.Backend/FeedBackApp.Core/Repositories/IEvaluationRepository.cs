
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IEvaluationRepository
    {
        Task<bool> UpdateOrSubmitQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire);
        Task<Questionnaire?> GetQuestionnaireByIdAsync(string id);
        Task<Questionnaire?> GetQuestionnaireByIdAsNoTrackingAsync(string id);
        Task<QuestionnaireTemplate?> GetQuestionnaresByIdAsNoTrackingAsync(string surevyId);
        Task<DateTime?> GetEndDateBySurveyId(string surveyId);

    }
}
