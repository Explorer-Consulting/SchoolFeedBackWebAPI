using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class EvaluationRepository : IEvaluationRepository
    {
        private readonly AppDBContext _context;

        public EvaluationRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateOrSubmitQuestionnaire(Questionnaire newQuestionnaire, Questionnaire oldQuestionnaire)
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
            return await _context.Questionnaires
                                 .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Questionnaire?> GetQuestionnaresByIdAsNoTrackingAsync(string id)
        {
            return await _context.Questionnaires
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QuestionnaireTemplate?> GetQuestionTemplateBySurveyIdAsync(string surveyId)
        {
            string id = $"questiontemplates_{surveyId}";
            return await _context.QuestionnaireTemplates
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(qt => qt.Id == id);
        }

        public async Task<DateTime?> GetEndDateBySurveyId(string surveyId)
        {
            if (!Guid.TryParse(surveyId, out var guid))
                return null;

            var survey = await _context.Surveys
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(s => s.Id == guid);

            return survey?.EndDate;
        }

        Task<DateTime> IEvaluationRepository.GetEndDateBySurveyId(string surveyId)
        {
            throw new NotImplementedException();
        }
    }
}
