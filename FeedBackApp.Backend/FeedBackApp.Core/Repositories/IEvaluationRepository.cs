
using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories
{
    public interface IEvaluationRepository
    {
        Task<bool> UpdateQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire);
        Task<Questionnaire> GetQuestionnaireByIdAsync(string id);
        Task<QuestionnaireTemplate> GetQuestionTemplateBySurveyIdAsync(string surevyId);
        Task<bool> SubmitQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire);

    }
}
