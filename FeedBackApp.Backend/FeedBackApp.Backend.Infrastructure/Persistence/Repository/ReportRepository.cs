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
    /// Riportok (PDF/Excel stb.) generálásáért és Azure Blob Storage-ba történő
    /// feltöltéséért felelős repository implementáció.
    /// <para>
    /// Nem tárol riport-metaadatot relációs adatbázisban; a riportok elérhetősége
    /// a blob elérési útjából / URL-jéből vezethető le.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para><b>Felelősség (SRP):</b> kizárólag riport létrehozás + feltöltés.</para>
    /// <para><b>Oldalsó hatás:</b> blobok jönnek létre/felülíródnak a megadott konténerben.</para>
    /// <para><b>Thread-safety:</b> az osztály példányai DI-n keresztül tipikusan
    /// scoped/transient életciklussal használhatók; a metódus nem tart állapotot
    /// hívások között.</para>
    /// </remarks>
    public sealed class ReportRepository(AppDBContext context, BlobContainerClient container) : IReportRepository
    {
        #region Dependencies

        /// <summary>
        /// Alkalmazás adatbázis-kontekstus (EF Core).
        /// </summary>
        private readonly AppDBContext _context = context;

        /// <summary>
        /// Az Azure Blob Storage konténer kliense, amelybe a riportok feltöltésre kerülnek.
        /// </summary>
        private readonly BlobContainerClient _container = container;

        #endregion

        #region Public API

        /// <summary>
        /// Azonosító alapján (pl. <c>questiontemplates_{GUID}</c>) összegyűjti az aktív kérdőíveket,
        /// legenerálja a kapcsolódó riportokat, és feltölti azokat a Blob Storage-ba.
        /// </summary>
        /// <param name="fullTemplateId">
        /// A kérdés-sablon teljes Cosmos dokumentumazonosítója. Elvárt formátum:
        /// <c>questiontemplates_{GUID}</c>.
        /// </param>
        /// <remarks>
        /// <para><b>Folyamat:</b></para>
        /// <list type="number">
        /// <item>Azonosító validálása (prefix + GUID).</item>
        /// <item>Aktív kérdőívek és válaszaik beolvasása az adott <c>surveyId</c>-ra.</item>
        /// <item>Válaszok csoportosítása tanár–tantárgy pár szerint.</item>
        /// <item>A sablonhoz tartozó kérdések beolvasása.</item>
        /// <item>Riportok legenerálása (<see cref="EvaluationReportCompiler"/>), feltöltése blobba.</item>
        /// </list>
        /// <para>
        /// A metódus nem dob hibát, ha nincs releváns adat (pl. nincs aktív kérdőív vagy kérdés-sablon);
        /// ilyenkor csendben visszatér.
        /// </para>
        /// <para><b>Teljesítmény:</b> a csoportosítás <c>GroupBy</c> segítségével történik memóriában;
        /// egy tanár–tantárgy párhoz lokális index épül a reportgenerálás során az értékelő
        /// util osztályban. A blob feltöltés <see cref="BinaryData"/>-val történik,
        /// ami közvetlenül a <c>byte[]</c> dokumentumot használja.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Ha az <paramref name="fullTemplateId"/> nem a várt formátumú (hibás prefix vagy érvénytelen GUID).
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

            // 1) Aktív kérdőívek + válaszaik betöltése (csak a tárgyalt survey-hez).
            var questionnaires = await _context.Questionnaires
                .AsNoTracking()
                .Where(q => q.Status == true && q.SurveyId == surveyId)
                .ToListAsync()
                .ConfigureAwait(false);

            if (questionnaires.Count == 0)
                return;

            var rows = questionnaires.Select(q => new
            {
                Teacher = new Teacher(q.TeacherEmail ?? string.Empty, q.SubjectName ?? string.Empty),
                Results = (q.QuestionnaireResults ?? Enumerable.Empty<QuestionAnswer>())
                    .Select(r => new QuestionAnswer
                    {
                        QuestionId = r.QuestionId,
                        Answer = r.Answer
                    })
                    .ToImmutableArray()
            }).ToList();

            // Nincs adat → nincs teendő.
            if (rows.Count == 0)
                return;

            // 3) Tanár–tantárgy kulcs alá aggregáljuk az összes választ.
            var answerCollection = rows
                .GroupBy(x => x.Teacher)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results).ToImmutableArray()
                );

            // 4) A sablonhoz tartozó kérdések betöltése.
            var template = await _context.QuestionnaireTemplates
                .AsNoTracking()
                .SingleOrDefaultAsync(qt => qt.Id == templateDocId)
                .ConfigureAwait(false);

            var questions = (template?.QuestionTemplates ?? [])
                .ToImmutableArray();

            // Ha nincs kérdés, nincs mit riportálni.
            if (questions.IsDefaultOrEmpty)
                return;

            // 5) Riportok generálása és feltöltése.
            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId))
            {
                var blobPath = BuildBlobPath($"{surveyId}_{document.Metadata.FileName}", document.Recipient);
                var blob = _container.GetBlobClient(blobPath);

                // Feltöltés BinaryData-val; a BinaryData-s overload implicit felülír, ha a blob létezik.
                await blob.UploadAsync(
                    BinaryData.FromBytes(document.Data),
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            // A megfelelő MIME-típus (pl. application/pdf, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet).
                            ContentType = document.Metadata.MimeType
                        }
                    }
                ).ConfigureAwait(false);
            }
        }

        #endregion

        #region Path helpers

        /// <summary>
        /// Képzi a riport blob útvonalát:
        /// <list type="bullet">
        /// <item><description><c>admin/&lt;fileName&gt;</c> – admin szintű, összesített riportok.</description></item>
        /// <item><description><c>teachers/&lt;safeEmail&gt;/&lt;fileName&gt;</c> – tanári riportok címzett e-mail szerinti alkönyvtárban.</description></item>
        /// </list>
        /// </summary>
        /// <param name="fileName">A feltöltendő fájl neve (kiterjesztéssel).</param>
        /// <param name="recipient">A címzett. Ha <see langword="null"/>, admin riportnak minősül.</param>
        /// <returns>Az Azure Blob Storage-beli relatív elérési út.</returns>
        private static string BuildBlobPath(string fileName, Recipient? recipient)
        {
            if (recipient is null)
                return $"admin/{fileName}";

            var safeEmail = San(recipient.EmailAddress);
            return $"teachers/{safeEmail}/{fileName}";
        }

        /// <summary>
        /// Útvonalkompatibilis e-mail/azonosító előállítása:
        /// a tiltott path-karaktereket kötőjelre (<c>-</c>) cseréli, majd <c>Trim()</c>-et alkalmaz.
        /// </summary>
        /// <param name="input">Eredeti e-mail vagy azonosító.</param>
        /// <returns>Biztonságosan használható path-szegmens (üres bemenet esetén üres string).</returns>
        private static string San(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Minimális tiltólista; igény szerint bővíthető (pl. *,"<> stb.), ha naming policy megköveteli.
            Span<char> invalid = ['/', '\\', '?', '#', '%', '+', '\t', '\r', '\n', ':'];

            var sb = new System.Text.StringBuilder(input.Length);
            foreach (var ch in input)
                sb.Append(invalid.Contains(ch) ? '-' : ch);

            return sb.ToString().Trim();
        }

        #endregion
    }
}
