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
    /// <summary>
    /// Repository responsible for generating reports (PDF/Excel, etc.) and uploading them to storage
    /// via <see cref="IBlobContext"/>.
    /// <para>
    /// It does not persist report metadata in a relational database; report availability can be inferred
    /// from the blob path / URL.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para><b>Responsibility (SRP):</b> report creation + upload only.</para>
    /// <para><b>Side effect:</b> blobs are created/overwritten in the specified container (handled by <see cref="IBlobContext"/>).</para>
    /// <para><b>Thread-safety:</b> instances are typically used via DI with scoped/transient lifetime;
    /// the method holds no state across calls.</para>
    /// </remarks>
    public sealed class ReportRepository(AppDBContext context, IBlobContext blob) : IReportRepository
    {
        #region Dependencies

        private readonly AppDBContext _context = context;
        private readonly IBlobContext _blob = blob;

        #endregion

        #region Public API

        /// <summary>
        /// Based on an identifier (e.g., <c>questiontemplates_{GUID}</c>) gathers active questionnaires,
        /// generates the corresponding reports, and uploads them to Blob Storage.
        /// </summary>
        /// <param name="fullTemplateId">
        /// The full Cosmos document ID of the question template. Expected format:
        /// <c>questiontemplates_{GUID}</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="fullTemplateId"/> is not in the expected format (wrong prefix or invalid GUID).
        /// </exception>
        public async Task CompileAndStoreEvaluationReports(string fullTemplateId)
        {
            const string prefix = "questiontemplates_";
            if (!fullTemplateId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid template ID format.", nameof(fullTemplateId));

            var guidPart = fullTemplateId[prefix.Length..];
            if (!Guid.TryParse(guidPart, out var surveyGuid))
                throw new ArgumentException("Invalid GUID in template ID.", nameof(fullTemplateId));

            var surveyId = surveyGuid.ToString("D");
            var templateDocId = fullTemplateId;

            // 1) Load ALL questionnaires by SurveyId ONLY (without Status!)
            var questionnairesRaw = await _context.Questionnaires
                .AsNoTracking()
                .Where(q => q.SurveyId == surveyId)
                .Select(q => new
                {
                    q.Status,
                    q.TeacherEmail,
                    q.SubjectName,
                    q.QuestionnaireResults
                })
                .ToListAsync()
                .ConfigureAwait(false);

            if (questionnairesRaw.Count == 0)
                return;

            // 2) In-memory Status filter
            var questionnaires = questionnairesRaw.Where(q => q.Status == true).ToList();
            //if (questionnaires.Count == 0)
            //    return;

            // 3) Normalize under teacher–subject key + project the inner collection in-memory
            var rows = questionnaires
                .Select(q => new
                {
                    Teacher = new Teacher(q.TeacherEmail ?? string.Empty, q.SubjectName ?? string.Empty),
                    Results = (q.QuestionnaireResults ?? Enumerable.Empty<QuestionAnswer>()).ToImmutableArray()
                })
                .ToList();

            //if (rows.Count == 0)
             //   return;

            // 4) Aggregate under teacher–subject key
            var answerCollection = rows
                .GroupBy(x => x.Teacher)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results).ToImmutableArray()
                );

            // 5) Load questions belonging to the template
            var template = await _context.QuestionnaireTemplates
                .AsNoTracking()
                .SingleOrDefaultAsync(qt => qt.Id == templateDocId)
                .ConfigureAwait(false);

            var questions = (template?.QuestionTemplates ?? []).ToImmutableArray();
            if (questions.IsDefaultOrEmpty)
                return;

            // 6) Generate and upload reports via IBlobContext
            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId))
            {
                var fileName = $"{surveyId}_{document.Metadata.FileName}";

                if (document.Recipient is null)
                {
                    await _blob.UploadAdminAsync(
                        fileName,
                        document.Data,
                        document.Metadata.MimeType
                    ).ConfigureAwait(false);
                }
                else
                {
                    await _blob.UploadTeacherAsync(
                        document.Recipient.EmailAddress,
                        fileName,
                        document.Data,
                        document.Metadata.MimeType
                    ).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}
