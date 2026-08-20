using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using CoreEmail = FeedBackApp.Core.Model.Email;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Cosmos-backed repository for survey authoring and questionnaire materialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Responsibilities</b><br/>
    /// Persists survey metadata and question templates, generates per-student questionnaires,
    /// maintains the email outbox document for student invitations, and exposes read/delete
    /// operations for surveys and related artifacts.
    /// </para>
    /// <para>
    /// <b>Storage model</b><br/>
    /// All aggregates are mapped by <see cref="AppDBContext"/> into a single Cosmos container
    /// using discriminators. <see cref="SurveyMetadata.Id"/> is the partition key for survey
    /// documents. Questionnaires and templates use their own identifiers (see method notes).
    /// </para>
    /// <para>
    /// <b>Consistency</b><br/>
    /// EF Core’s Cosmos provider issues individual requests per entity group during
    /// <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>; operations are not transactional
    /// across multiple logical documents unless explicitly batched with transactions (not used here).
    /// </para>
    /// </remarks>
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initializes the repository with an EF Core Cosmos database context.
        /// </summary>
        /// <param name="context">Cosmos-configured EF Core <see cref="DbContext"/>.</param>
        public QuestionnaireRepository(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Compiles and persists a complete survey: saves metadata, creates a template bundle,
        /// materializes per-student questionnaires, and enqueues student invitation emails.
        /// </summary>
        /// <param name="metadata">The fully validated survey metadata to persist and expand.</param>
        /// <remarks>
        /// <para>
        /// <b>Questionnaire IDs</b><br/>
        /// Each questionnaire receives an identifier composed as:<br/>
        /// <c>{studentEmail}_{teacherEmail}_{subjectName}_{surveyId}</c>.
        /// </para>
        /// <para>
        /// <b>Template document</b><br/>
        /// A <see cref="QuestionnaireTemplate"/> is created with id <c>questiontemplates_{surveyId}</c> that holds
        /// the survey’s <see cref="QuestionTemplate"/> list.
        /// </para>
        /// <para>
        /// <b>Email outbox</b><br/>
        /// Student invitation recipients are added to the singleton document with id <c>emailsToSend</c>.
        /// If the document does not exist, it is created.
        /// </para>
        /// </remarks>
        public async Task CompileAndSaveAsync(SurveyMetadata metadata)
        {
            var setById = metadata.StudentSets.ToDictionary(s => s.SetId);
            var template = metadata.QuestionTemplates;

            QuestionnaireTemplate tempForSave =
                new QuestionnaireTemplate(metadata.Id.ToString(), metadata.Title, template)
                {
                    RequireValidation = metadata.RequireValidation
                };

            _context.Add(metadata);
            _context.Add(tempForSave);

            var questionnaires = new List<Questionnaire>();
            var allEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var questionnaireIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var param in metadata.CreationParams)
            {
                foreach (var setId in param.StudentSetIds)
                {
                    if (!setById.TryGetValue(setId, out var set))
                        continue;

                    foreach (var studentEmail in set.StudentEmails)
                    {
                        allEmails.Add(studentEmail);

                        var qId = $"{studentEmail}_{param.TeacherEmail}_{param.SubjectName}_{metadata.Id}";

                        if (!questionnaireIds.Add(qId))
                            continue;

                        var q = new Questionnaire
                        {
                            Id = qId,
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

            var newEmailEntry = new CoreEmail
            {
                SurveyId = metadata.Id.ToString(),
                SurveyName = metadata.Title,
                StartDate = metadata.StartDate,
                EndDate = metadata.EndDate,
                Emails = allEmails.ToList(),
                Role = Core.Model.Enum.Role.Student
            };

            if (emailDoc == null)
            {
                emailDoc = new EmailsToSend
                {
                    Id = "emailsToSend",
                    EmailsToSendList = new List<CoreEmail> { newEmailEntry }
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


        /// <summary>
        /// Deletes all questionnaires belonging to a survey.
        /// </summary>
        /// <param name="surveyId">The survey identifier whose questionnaires should be removed.</param>
        /// <returns>
        /// <c>true</c> if at least one questionnaire was deleted; otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Deletes the question template bundle associated with the specified survey.
        /// </summary>
        /// <param name="surveyId">Survey identifier tied to the template set.</param>
        /// <returns>
        /// <c>true</c> if the template bundle existed and was removed; otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Deletes a survey metadata document.
        /// </summary>
        /// <param name="id">Survey identifier.</param>
        /// <returns><c>true</c> if the survey existed and was deleted; otherwise <c>false</c>.</returns>
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

        /// <summary>
        /// Retrieves all surveys (no filtering), projected as tracked-off snapshots.
        /// </summary>
        /// <returns>List of <see cref="SurveyMetadata"/>.</returns>
        public async Task<List<SurveyMetadata>> GetAllSurveyMetadata()
        {
            return await _context.Surveys
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a questionnaire by its composite identifier.
        /// </summary>
        /// <param name="id">Questionnaire id formatted as <c>{studentEmail}_{teacherEmail}_{subject}_{surveyId}</c>.</param>
        /// <returns>The questionnaire if found; otherwise <c>null</c>.</returns>
        public async Task<Questionnaire?> GetQuestionnaireByIdAsync(string id)
        {
            return await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        /// <summary>
        /// Retrieves a questionnaire by its composite identifier and the object is being tracked by EF.
        /// </summary>
        /// <param name="id">Questionnaire id formatted as <c>{studentEmail}_{teacherEmail}_{subject}_{surveyId}</c>.</param>
        /// <returns>The questionnaire if found; otherwise <c>null</c>.</returns>
        public async Task<Questionnaire?> GetQuestionnaireByIdWithTrackingAsync(string id)
        {
            return await _context.Questionnaires.FirstOrDefaultAsync(q => q.Id == id);
        }

        /// <summary>
        /// Retrieves survey metadata by its identifier.
        /// </summary>
        /// <param name="surveyId">Survey identifier.</param>
        /// <returns>The survey metadata if found; otherwise <c>null</c>.</returns>
        public async Task<SurveyMetadata?> GetSurveyMetadataAsync(Guid surveyId)
        {
            return await _context.Surveys
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);
        }

        /// <summary>
        /// Retrieves surveys visible to a student based on membership in any configured student set.
        /// </summary>
        /// <param name="studentEmail">Student email used for set membership evaluation.</param>
        /// <returns>Surveys that include the student in at least one set (no date filtering applied here).</returns>
        public async Task<List<SurveyMetadata>> GetSurveyMetadataForStudentAsync(string studentEmail)
        {
            var allSurveys = await _context.Surveys
                .AsNoTracking()
                .ToListAsync();

            var activeSurveys = allSurveys
                .Where(s => s.StudentSets.Any(set =>
                    set.StudentEmails.Contains(studentEmail, StringComparer.OrdinalIgnoreCase)))
                .ToList();

            return activeSurveys;
        }

        /// <summary>
        /// Updates an existing questionnaire document in the database.
        /// </summary>
        /// <param name="questionnaire"></param>
        /// <returns></returns>
        public async Task UpdateQuestionnaireAsync(Questionnaire questionnaire)
        {
            _context.Update(questionnaire);
            await _context.SaveChangesAsync();
        }
    }
}
