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
    /// Riportok generálása és feltöltése Azure Blob Storage-ba.
    /// Nem tárol metaadatot adatbázisban – a riportok elérhetősége a blob útvonalból/URL-ből nyerhető ki.
    /// </summary>
    public sealed class ReportRepository(AppDBContext context, BlobServiceClient blob) : IReportRepository
    {
        private readonly AppDBContext _context = context;
        private readonly BlobServiceClient _blob = blob;

        /// <summary>
        /// A megadott sablon-azonosítóhoz tartozó (questiontemplates_{guid}) aktív válaszokból riportokat generál,
        /// majd a kész dokumentumokat feltölti a Blob Storage-ba.
        /// </summary>
        /// <param name="fullTemplateId">A sablon dokumentum teljes Cosmos ID-ja (pl. <c>questiontemplates_d382f858-...</c>).</param>
        /// <exception cref="ArgumentException">Érvénytelen ID-formátum esetén.</exception>
        /// <exception cref="InvalidOperationException">Hiányzó környezeti változók esetén.</exception>
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

            var rows = await _context.Questionnaires
                .AsNoTracking()
                .Where(q => q.Status && q.SurveyId == surveyId)
                .Select(q => new
                {
                    Teacher = new Teacher(q.TeacherEmail, q.SubjectName),
                    Results = q.QuestionnaireResults.Select(r => new QuestionAnswer
                    {
                        QuestionId = r.QuestionId,
                        Answer = r.Answer
                    })
                })
                .ToListAsync();

            if (rows.Count == 0)
                return;

            var answerCollection = rows
                .GroupBy(x => x.Teacher)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results).ToImmutableArray()
                );

            var questions = (await _context.QuestionnaireTemplates
                    .AsNoTracking()
                    .Where(qt => qt.Id == templateDocId)
                    .SelectMany(qt => qt.QuestionTemplates)
                    .ToListAsync())
                .ToImmutableArray();

            if (questions.IsDefaultOrEmpty)
                return;

            var containerName = Environment.GetEnvironmentVariable("AZURE_REPORTS_CONTAINER")
                ?? throw new InvalidOperationException("AZURE_REPORTS_CONTAINER environment variable is not set.");

            var container = _blob.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId))
            {
                var blobPath = BuildBlobPath(document.Metadata.FileName, document.Recipient);
                var blob = container.GetBlobClient(blobPath);

                // ha már létezik, töröljük (felülírás logika)
                await blob.DeleteIfExistsAsync();

                using var ms = new MemoryStream(document.Data, writable: false);
                await blob.UploadAsync(
                    ms,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = document.Metadata.MimeType
                        }
                    }
                );

            }
        }

        /// <summary>
        /// Admin riportot az „admin/” alá, tanári riportot e-mail szerinti alkönyvtárba helyez.
        /// </summary>
        private static string BuildBlobPath(string fileName, Recipient? recipient)
        {
            if (recipient is null)
                return $"admin/{fileName}";

            var safeEmail = San(recipient.EmailAddress);
            return $"teachers/{safeEmail}/{fileName}";
        }

        /// <summary>
        /// Tiltott útvonal-karakterek cseréje kötőjelre.
        /// </summary>
        private static string San(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            Span<char> invalid = ['/', '\\', '?', '#', '%', '+', '\t', '\r', '\n', ':'];
            var sb = new System.Text.StringBuilder(input.Length);
            foreach (var ch in input)
                sb.Append(invalid.Contains(ch) ? '-' : ch);
            return sb.ToString().Trim();
        }
    }
}
