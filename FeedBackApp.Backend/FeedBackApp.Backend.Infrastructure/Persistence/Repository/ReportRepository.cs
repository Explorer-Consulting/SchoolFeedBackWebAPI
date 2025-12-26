using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public sealed class ReportRepository(AppDBContext context, IBlobContext blob) : IReportRepository
    {
        private readonly AppDBContext _context = context;
        private readonly IBlobContext _blob = blob;

        public async Task CompileAndStoreEvaluationReports(string fullTemplateId)
        {
            const string prefix = "questiontemplates_";
            if (!fullTemplateId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid template ID format.", nameof(fullTemplateId));

            if (!Guid.TryParse(fullTemplateId[prefix.Length..], out var surveyGuid))
                throw new ArgumentException("Invalid GUID in template ID.", nameof(fullTemplateId));

            var surveyId = surveyGuid.ToString("D");
            var templateDocId = fullTemplateId;

            // 1) Questionnaires (kész értékelések) betöltése
            var questionnairesQuery = EntityFrameworkQueryableExtensions.AsNoTracking(_context.Questionnaires);

            var questionnaires = await questionnairesQuery
                .Where(q => q.SurveyId == surveyId && q.Status == true)
                .Select(q => new
                {
                    q.TeacherEmail,
                    q.SubjectName,
                    Results = q.QuestionnaireResults
                })
                .ToListAsync();

            if (questionnaires.Count == 0)
                return;

            var rows = questionnaires
                .Select(q => new
                {
                    Teacher = new Teacher(q.TeacherEmail ?? string.Empty, q.SubjectName ?? string.Empty),
                    Results = (q.Results ?? []).ToImmutableArray()
                })
                .Where(x => x.Results.Length > 0)
                .ToList();

            if (rows.Count == 0)
                return;

            var answerCollection = rows
                .GroupBy(x => x.Teacher)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results).ToImmutableArray()
                );

            // 2) Template kérdések betöltése (Cosmos: point-read, ha Id a PK)
            var templatesQuery = EntityFrameworkQueryableExtensions.AsNoTracking(_context.QuestionnaireTemplates);

            // FindAsync akkor a legjobb, ha a key/PK az Id (nálad ez van)
            var template = await templatesQuery
                .Where(x => x.Id == templateDocId)
                .SingleOrDefaultAsync();

            var questions = (template?.QuestionTemplates ?? new List<QuestionTemplate>()).ToImmutableArray();
            if (questions.IsDefaultOrEmpty)
                return;

            // 3) Generálás + feltöltés
            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId))
            {
                var fileName = $"{surveyId}_{document.Metadata.FileName}";

                if (document.Recipient is null)
                {
                    await _blob.UploadAdminAsync(fileName, document.Data, document.Metadata.MimeType);
                }
                else
                {
                    await _blob.UploadTeacherAsync(document.Recipient.EmailAddress, fileName, document.Data, document.Metadata.MimeType);
                }
            }
        }
    }
}
