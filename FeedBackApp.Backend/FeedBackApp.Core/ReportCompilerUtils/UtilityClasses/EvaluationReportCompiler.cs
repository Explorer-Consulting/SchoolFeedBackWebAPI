using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.UtilityClasses
{
    /// <summary>
    /// Riportok összeállítása a beérkezett kérdőív-válaszokból.
    /// <para>
    /// Tanáronkénti és globális (adminisztrátori) riportokat állít elő PDF/Excel formátumban.
    /// A kérdések típusai alapján a megfelelő kiértékelési modelleket és komponenseket illeszti be.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Fő lépések:
    /// <list type="number">
    /// <item>Válaszok indexelése kérdés-azonosító szerint.</item>
    /// <item>Adatgyűjtés kérdéstípusonként (Likert, single/multi choice, open-ended).</item>
    /// <item>Komponensek összeállítása és (opcionálisan) kiértékelése.</item>
    /// <item>Dokumentumok renderelése és <see cref="IAsyncEnumerable{ReportDocument}"/> formában streamelése.</item>
    /// </list>
    /// </remarks>
    public static class EvaluationReportCompiler
    {
        /// <summary>
        /// Válaszok indexelése kérdésazonosító (<see cref="QuestionAnswer.QuestionId"/>) szerint.
        /// </summary>
        /// <param name="answers">A kitöltőktől érkezett válaszok.</param>
        /// <returns>Szótár, ahol kulcs a kérdés ID, érték a kapcsolódó válaszok listája.</returns>
        private static IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> BuildAnswersIndex(ImmutableArray<QuestionAnswer> answers)
        {
            return answers
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.ToImmutableArray());
        }

        /// <summary>
        /// Likert-skálás válaszok (egész értékek) összegyűjtése egy kérdéshez.
        /// </summary>
        private static ImmutableArray<int> CollectLikertScaleData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>(list.Length);
            foreach (var a in list)
                if (int.TryParse(a.Answer, out var v)) b.Add(v);
            return b.MoveToImmutable();
        }

        /// <summary>
        /// Egyválasztós (index alapú) válaszok összegyűjtése.
        /// </summary>
        private static ImmutableArray<int> CollectSingleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>(list.Length);
            foreach (var a in list)
                if (int.TryParse(a.Answer, out var v)) b.Add(v);
            return b.MoveToImmutable();
        }

        /// <summary>
        /// Egyéni/szabad szöveges „Egyéb” válaszok összegyűjtése (whitespace szűréssel).
        /// </summary>
        private static ImmutableArray<string> CollectCustomSingleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<string>(list.Length);
            foreach (var a in list)
                if (!string.IsNullOrWhiteSpace(a.Answer)) b.Add(a.Answer);
            return b.MoveToImmutable();
        }

        /// <summary>
        /// Többválasztós válaszok összegyűjtése (több index egy mezőben, kötőjellel elválasztva).
        /// </summary>
        private static ImmutableArray<int> CollectMultipleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var buf = new List<int>(list.Length * 3);
            foreach (var a in list)
            {
                if (string.IsNullOrWhiteSpace(a.Answer)) continue;
                foreach (var token in a.Answer.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    if (int.TryParse(token, out var v)) buf.Add(v);
            }
            return [.. buf];
        }

        /// <summary>
        /// Nyílt végű (szöveges) válaszok összegyűjtése (whitespace szűréssel).
        /// </summary>
        private static ImmutableArray<string> CollectOpenEndedData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<string>(list.Length);
            foreach (var a in list)
                if (!string.IsNullOrWhiteSpace(a.Answer)) b.Add(a.Answer);
            return b.MoveToImmutable();
        }

        /// <summary>
        /// Kérdések komponenseinek összeállítása és opcionális kiértékelése.
        /// <para>
        /// A <paramref name="evaluate"/> jelzi, hogy az adatmodelleken futtassuk-e az
        /// <see cref="EvaluationData.EvaluateData"/> metódust (pl. PDF-hez), vagy nyers komponenseket adjunk vissza (pl. Excelhez).
        /// </para>
        /// </summary>
        /// <param name="document">A cél dokumentum (PDF/Excel), amelynek <see cref="ReportDocument.ReportComponents"/> listájába építkezünk.</param>
        /// <param name="questions">A kérdéssablonok sorozata.</param>
        /// <param name="index">Válaszindex kérdés-ID szerint.</param>
        /// <param name="evaluate"><c>true</c> esetén statisztikák számítása is történik.</param>
        /// <returns>Ugyanaz a <paramref name="document"/> példány, kiegészítve a komponensekkel.</returns>
        private static ReportDocument CompileQuestions(
            ReportDocument document,
            ImmutableArray<QuestionTemplate> questions,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index,
            bool evaluate)
        {
            foreach (var q in questions)
            {
                switch (q.Type)
                {
                    case QuestionType.LikertScaleOneToFive:
                        {
                            var ed = new LikertScaleEvaluationData(q.Question, CollectLikertScaleData(q.Id, index), q.Description, 1, 5);
                            document.ReportComponents.Add((evaluate ? ed.EvaluateData() : ed).CompileComponent());
                            break;
                        }

                    case QuestionType.MultinomialSingleChoice:
                        {
                            var ed = new SingleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], SingleChoice.REGULAR, CollectSingleChoiceData(q.Id, index), []);
                            document.ReportComponents.Add((evaluate ? ed.EvaluateData() : ed).CompileComponent());
                            break;
                        }

                    case QuestionType.MultiNomialSingleChoiceOther:
                        {
                            var ed = new SingleChoiceEvaluationData(q.Question, [], SingleChoice.CUSTOM, [], CollectCustomSingleChoiceData(q.Id, index));
                            document.ReportComponents.Add((evaluate ? ed.EvaluateData() : ed).CompileComponent());
                            break;
                        }

                    case QuestionType.MultipleChoice:
                        {
                            var ed = new MultipleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], CollectMultipleChoiceData(q.Id, index));
                            document.ReportComponents.Add((evaluate ? ed.EvaluateData() : ed).CompileComponent());
                            break;
                        }

                    case QuestionType.OpenEnded:
                        {
                            var ed = new OpenEndedEvaluationData(q.Question, CollectOpenEndedData(q.Id, index));
                            document.ReportComponents.Add((evaluate ? ed.EvaluateData() : ed).CompileComponent());
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(q.Type), q.Type, "Ismeretlen kérdéstípus.");
                }
            }

            return document;
        }

        /// <summary>
        /// Teljes riportkészítés streamelve: tanáronkénti PDF-ek, globális PDF és globális Excel.
        /// <para>
        /// A riportok <b>lustán</b> kerülnek előállításra és <see cref="yield"/>-del adjuk vissza őket,
        /// így a hívó azonnal feldolgozhatja/feltöltheti az elkészült dokumentumokat.
        /// </para>
        /// </summary>
        /// <param name="rawData">Tanárokhoz rendelt válaszok gyűjteménye.</param>
        /// <param name="rawQuestions">A kitöltött kérdések sablonjai (szöveg, opciók, típusok).</param>
        /// <returns><see cref="IAsyncEnumerable{ReportDocument}"/> a legenerált dokumentumokkal.</returns>
        public static async IAsyncEnumerable<ReportDocument> CompileReports(
        ImmutableDictionary<Teacher, ImmutableArray<QuestionAnswer>> rawData,
        ImmutableArray<QuestionTemplate> rawQuestions,
        string surveyId) // maradhat, fájlnévhez hasznos
        {
            ArgumentNullException.ThrowIfNull(rawData);
            ArgumentNullException.ThrowIfNull(rawQuestions);
            ArgumentException.ThrowIfNullOrEmpty(surveyId);

            // 1) Tanáronkénti PDF-ek
            foreach (var entry in rawData)
            {
                var teacher = entry.Key;
                var answers = entry.Value;
                var idx = BuildAnswersIndex(answers);

                var safeTeacher = San(teacher.EmailAddress);
                var safeSubject = San(teacher.SubjectName);

                string fileName = $"{safeTeacher}_{safeSubject}_report.pdf";

                var metadata = new ReportMetadata
                {
                    MimeType = "application/pdf",
                    FileName = fileName,
                    Author = "Explorer Consulting",
                    BLOB_URI = string.Empty
                };

                var doc = new TeacherPDFReportDocument(metadata, teacher);
                var compiled = CompileQuestions(doc, rawQuestions, idx, evaluate: true);
                await compiled.RenderDocument();
                yield return compiled;
            }

            // 2/a) Globális PDF
            {
                var allData = rawData.Values.SelectMany(x => x).ToImmutableArray();
                var globalIndex = BuildAnswersIndex(allData);

                const string fileName = "global_report.pdf";
                var metadata = new ReportMetadata
                {
                    MimeType = "application/pdf",
                    FileName = fileName,
                    Author = "Explorer Consulting",
                    BLOB_URI = string.Empty
                };

                var adminPdf = new AdministratorPDFReportDocument(metadata);
                var compiledPdf = CompileQuestions(adminPdf, rawQuestions, globalIndex, evaluate: true);
                await compiledPdf.RenderDocument();
                yield return compiledPdf;
            }

            // 2/b) Globális Excel (nyers)
            {
                const string fileName = "global_report.xlsx";
                var metadata = new ReportMetadata
                {
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = fileName,
                    Author = "Explorer Consulting",
                    BLOB_URI = string.Empty
                };

                var adminExcel = new AdministratorExcelReportDocument(metadata);
                var allData = rawData.Values.SelectMany(x => x).ToImmutableArray();
                var globalIndex = BuildAnswersIndex(allData);
                var compiledExcel = CompileQuestions(adminExcel, rawQuestions, globalIndex, evaluate: false);
                await compiledExcel.RenderDocument();
                yield return compiledExcel;
            }
        }

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
