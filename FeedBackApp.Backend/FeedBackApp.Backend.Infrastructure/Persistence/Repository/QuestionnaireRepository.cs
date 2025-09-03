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

            QuestionnaireTemplate tempForSave = new QuestionnaireTemplate(metadata.Id.ToString(), template);

            _context.Add(metadata);
            _context.Add(tempForSave);

            var questionnaires = new List<Questionnaire>();
            var allEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var param in metadata.CreationParams)
            {
                foreach (var setId in param.StudentSetIds)
                {
                    if (!setById.TryGetValue(setId, out var set))
                        continue;

                    foreach (var studentEmail in set.StudentEmails)
                    {
                        allEmails.Add(studentEmail);
                        var q = new Questionnaire
                        {
                            Id = $"{studentEmail}_{param.TeacherEmail}_{param.SubjectName}_{metadata.Id}",
                            SurveyId = metadata.Id.ToString(),
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

            var emailDoc = await _context.EmailsToSend
                .FirstOrDefaultAsync(e => e.Id == "emailsToSend");

            var newEmailEntry = new Email
            {
                SurveyId = metadata.Id.ToString(),
                SurveyName = metadata.Title,
                StartDate = metadata.StartDate,
                EndDate = metadata.EndDate,
                Emails = allEmails.ToList()
            };

            if (emailDoc == null)
            {
                // First time: create the document
                emailDoc = new EmailsToSend
                {
                    Id = "emailsToSend",
                    EmailsToSendList = new List<Email> { newEmailEntry }
                };

                _context.Add(emailDoc);
            }
            else
            {
                emailDoc.EmailsToSendList.Add(newEmailEntry);
                _context.Update(emailDoc);
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
            var questionTemplate = await _context.QuestionnaireTemplates
                .FirstOrDefaultAsync(q => q.Id == $"questiontemplates_{surveyId}");

            if (questionTemplate == null)
            {
                return false;
            }

            _context.Remove(questionTemplate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSurveyMetadataAsync(Guid id)
        {
            var metadata = await _context.Surveys
                .FirstOrDefaultAsync(m => m.Id == id);

            if (metadata == null)
                return false;

            _context.Remove(metadata);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SurveyMetadata>> GetAllSurveyMetadata()
        {
            return await _context.Surveys
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Questionnaire?> GetQuestionnaireByIdAsync(string id)
        {
            return await _context.Questionnaires
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<SurveyMetadata?> GetSurveyMetadataAsync(Guid surveyId)
        {
            return await _context.Surveys
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);
        }

        public async Task<List<SurveyMetadata>> GetSurveyMetadataForStudentAsync(string studentEmail)
        {
            return await _context.Surveys
                .AsNoTracking()
                .Where(s => s.StudentSets
                    .Any(set => set.StudentEmails.Contains(studentEmail)))
                .ToListAsync();
        }
    }
}
