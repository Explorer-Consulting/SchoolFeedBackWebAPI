using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class EvaluationRepository : IEvaluationRepository
    {
        private readonly AppDBContext _context;

        public EvaluationRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire)
        {
            return await UpdateAnswersAndSaveAsync(newQuestionnaire, oldQuestionnaire);
        }

        public async Task<bool> SubmitQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire)
        {
            return await UpdateAnswersAndSaveAsync(newQuestionnaire, oldQuestionnaire);
        }

        private async Task<bool> UpdateAnswersAndSaveAsync(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire)
        {
            foreach (var oldAnswer in oldQuestionnaire.QuestionnaireResults)
            {
                var newAnswer = newQuestionnaire.QuestionnaireResults
                    .FirstOrDefault(x => x.QuestionId == oldAnswer.QuestionId);

                if (newAnswer == null)
                    return false;

                oldAnswer.Answer = newAnswer.Answer;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Questionnaire?> GetQuestionnaireByIdAsync(string id)
        {
            return await _context.Questionnaires.FindAsync(id) ?? null;
        }

        public async Task<QuestionnaireTemplate?> GetQuestionTemplateBySurveyIdAsync(string surveyId)
        {
            return await _context.QuestionnaireTemplates.FindAsync($"questiontemplates_{surveyId}") ?? null;
        }

        public async Task<DateTime?> GetEndDateBySurveyId(string surveyId)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);
            return survey?.EndDate;
        }
    }
}
