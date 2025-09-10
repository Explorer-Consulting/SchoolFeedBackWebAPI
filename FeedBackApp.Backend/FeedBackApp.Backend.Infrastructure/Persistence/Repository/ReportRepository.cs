using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository responsible for generating reports (PDF/Excel, etc.) and uploading them to Azure Blob Storage.
    /// <para>
    /// It does not persist report metadata in a relational database; report availability can be inferred
    /// from the blob path / URL.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para><b>Responsibility (SRP):</b> report creation + upload only.</para>
    /// <para><b>Side effect:</b> blobs are created/overwritten in the specified container.</para>
    /// <para><b>Thread-safety:</b> instances are typically used via DI with scoped/transient lifetime;
    /// the method holds no state across calls.</para>
    /// </remarks>
    public sealed class ReportRepository(AppDBContext context, BlobContainerClient container) : IReportRepository
    {
        #region Dependencies

        /// <summary>
        /// Application database context (EF Core).
        /// </summary>
        private readonly AppDBContext _context = context;

        /// <summary>
        /// Azure Blob Storage container client where reports are uploaded.
        /// </summary>
        private readonly BlobContainerClient _container = container;

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
        /// <remarks>
        /// <para><b>Process:</b></para>
        /// <list type="number">
        /// <item>Validate identifier (prefix + GUID).</item>
        /// <item>Load active questionnaires and their responses for the given <c>surveyId</c>.</item>
        /// <item>Group responses by teacher–subject pair.</item>
        /// <item>Load questions that belong to the template.</item>
        /// <item>Generate reports (<see cref="EvaluationReportCompiler"/>), then upload them to blobs.</item>
        /// </list>
        /// <para>
        /// The method does not throw if there is no relevant data (e.g., no active questionnaire or question template);
        /// it simply returns.
        /// </para>
        /// <para><b>Performance:</b> grouping is performed in-memory via <c>GroupBy</c>;
        /// for each teacher–subject pair, a local index is built during report generation in the evaluation utility.
        /// Blob upload uses <see cref="BinaryData"/>, which directly wraps the document <c>byte[]</c>.
        /// </para>
        /// </remarks>
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
            //    Project only required fields to keep the payload thin.
            //    IMPORTANT: do not .Select(...) inner collections here; do that in-memory.
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
            if (questionnaires.Count == 0)
                return;

            // 3) Normalize under teacher–subject key + project the inner collection in-memory
            var rows = questionnaires
                .Select(q => new
                {
                    Teacher = new Teacher(q.TeacherEmail ?? string.Empty, q.SubjectName ?? string.Empty),

                    // If inner items are already QuestionAnswer, we can just make them immutable:
                    Results = (q.QuestionnaireResults ?? Enumerable.Empty<QuestionAnswer>()).ToImmutableArray()

                    // If truly new instances were needed:
                    // Results = (q.QuestionnaireResults ?? Enumerable.Empty<QuestionAnswer>())
                    //     .Select(r => new QuestionAnswer { QuestionId = r.QuestionId, Answer = r.Answer })
                    //     .ToImmutableArray()
                })
                .ToList();

            if (rows.Count == 0)
                return;

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

            // 5) Generate and upload reports
            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId))
            {
                var blobPath = BuildBlobPath($"{surveyId}_{document.Metadata.FileName}", document.Recipient);
                var blob = _container.GetBlobClient(blobPath);

                // Upload using BinaryData; this overload implicitly overwrites if the blob exists.
                await blob.UploadAsync(
                    BinaryData.FromBytes(document.Data),
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            // Proper MIME type (e.g., application/pdf, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet).
                            ContentType = document.Metadata.MimeType
                        }
                    }
                ).ConfigureAwait(false);
            }
        }

        #endregion

        #region Path helpers

        /// <summary>
        /// Builds the blob path for a report:
        /// <list type="bullet">
        /// <item><description><c>admin/&lt;fileName&gt;</c> – admin-level, aggregated reports.</description></item>
        /// <item><description><c>teachers/&lt;safeEmail&gt;/&lt;fileName&gt;</c> – teacher reports under a directory named by the recipient email.</description></item>
        /// </list>
        /// </summary>
        /// <param name="fileName">Name of the file to upload (including extension).</param>
        /// <param name="recipient">The recipient. If <see langword="null"/>, treated as an admin report.</param>
        /// <returns>Relative path within Azure Blob Storage.</returns>
        private static string BuildBlobPath(string fileName, Recipient? recipient)
        {
            if (recipient is null)
                return $"admin/{fileName}";

            var safeEmail = San(recipient.EmailAddress);
            return $"teachers/{safeEmail}/{fileName}";
        }

        /// <summary>
        /// Produces a path-compatible email/identifier:
        /// replaces forbidden path characters with a hyphen (<c>-</c>) and applies <c>Trim()</c>.
        /// </summary>
        /// <param name="input">Original email or identifier.</param>
        /// <returns>A path-safe segment (empty string when input is null/whitespace).</returns>
        private static string San(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Minimal denylist; extend as needed (*,"<> etc.) if your naming policy requires.
            Span<char> invalid = ['/', '\\', '?', '#', '%', '+', '\t', '\r', '\n', ':'];

            var sb = new System.Text.StringBuilder(input.Length);
            foreach (var ch in input)
                sb.Append(invalid.Contains(ch) ? '-' : ch);

            return sb.ToString().Trim();
        }

        #endregion
    }
}
