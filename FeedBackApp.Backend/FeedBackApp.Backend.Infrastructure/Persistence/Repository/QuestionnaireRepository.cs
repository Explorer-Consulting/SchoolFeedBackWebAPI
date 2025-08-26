
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly AppDBContext _context;

        public QuestionnaireRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task CompileAndSaveAsync(SurveyMetadata metadata)
        {
            var setById = metadata.StudentSets.ToDictionary(s => s.SetId);
            var template = metadata.QuestionTemplates;

            QuestionnaireTemplate tempForSave = new QuestionnaireTemplate(metadata.Id, template);

            _context.Add(metadata);
            _context.Add(tempForSave);

            var questionnaires = new List<Questionnaire>();

            foreach (var param in metadata.CreationParams)
            {
                foreach (var setId in param.StudentSetIds)
                {
                    if (!setById.TryGetValue(setId, out var set))
                        continue;

                    foreach (var studentEmail in set.StudentEmails)
                    {
                        var q = new Questionnaire
                        {
                            Id = $"{studentEmail}_{param.TeacherEmail}_{param.SubjectName}_{metadata.Id}",
                            SurveyId = metadata.Id,
                            TeacherEmail = param.TeacherEmail,
                            StudentEmail = studentEmail,
                            SubjectName = param.SubjectName,
                            QuestionnaireResults = template
                                .Select(t => new QuestionAnswer
                                {
                                    Answer = string.Empty,
                                    QuestionId = t.Id
                                })
                                .ToList()
                        };

                        questionnaires.Add(q);
                    }
                }
            }

            if (questionnaires.Count > 0)
            {
                _context.AddRange(questionnaires);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteQuestionnairesBySurveyIdAsync(Guid surveyId)
        {
            var questionnaires = await _context.Questionnaires
                .Where(q => q.SurveyId == surveyId.ToString())
                .ToListAsync();

            if (!questionnaires.Any())
                return false;

            _context.Questionnaires.RemoveRange(questionnaires);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionTemplateBySurveyIdAsync(Guid surveyId)
        {
            var questionTemplate = await _context.QuestionnnareTemplates
                .FirstAsync(q => q.Id == $"questiontemplates_{surveyId}");

            if(questionTemplate == null)
            {
                return false;
            }

            _context.Remove(questionTemplate);
            await _context.SaveChangesAsync();
            return true;
                
        }

        public async Task<bool> DeleteSurveyMetadataAsync(Guid id)
        {
            var metadata = await _context.Surveys.FirstOrDefaultAsync(m => m.Id == id.ToString());
            if (metadata == null)
                return false;

            _context.Remove(metadata);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Questionnaire?> GetQuestionnaireByIdAsync(string id)
        {
            var questionnair = await _context.Questionnaires.FirstOrDefaultAsync(questionnair => questionnair.Id == id);
            return questionnair;
        }

        public async Task<SurveyMetadata?> GetSurveyMetadataAsync(Guid surveyId)
        {
            var metadata = await _context.Surveys.FirstOrDefaultAsync(survey => survey.Id == surveyId.ToString());
            return metadata;
        }
    }
}
