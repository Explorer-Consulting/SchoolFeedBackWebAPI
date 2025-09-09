using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Collections.Immutable;
using System.Globalization;

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
        // --- Helpers ---
        private static bool HasData<T>(ImmutableArray<T> data) => !data.IsDefault && data.Length > 0;

        /// <summary>
        /// Válaszok indexelése kérdésazonosító (<see cref="QuestionAnswer.QuestionId"/>) szerint.
        /// </summary>
        private static IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> BuildAnswersIndex(ImmutableArray<QuestionAnswer> answers) => answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToImmutableArray());

        /// <summary>
        /// Likert-skálás válaszok (egész értékek) összegyűjtése egy kérdéshez.
        /// </summary>
        private static ImmutableArray<int> CollectLikertScaleData(
            string id,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>(); // nincs fix kapacitás, mert szűrünk
            foreach (var a in list)
                if (int.TryParse(a.Answer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    b.Add(v);

            return b.ToImmutable();
        }

        /// <summary>
        /// Egyválasztós (index alapú) válaszok összegyűjtése.
        /// </summary>
        private static ImmutableArray<int> CollectSingleChoiceData(
            string id,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>();
            foreach (var a in list)
                if (int.TryParse(a.Answer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    b.Add(v);

            return b.ToImmutable();
        }

        /// <summary>
        /// Nyílt végű (szöveges) válaszok összegyűjtése (whitespace szűréssel).
        /// </summary>
        private static ImmutableArray<string> CollectOpenEndedData(
            string id,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<string>();
            foreach (var a in list)
                if (!string.IsNullOrWhiteSpace(a.Answer))
                    b.Add(a.Answer!);

            return b.ToImmutable();
        }

        /// <summary>
        /// Egyéni/szabad szöveges „Egyéb” válaszok összegyűjtése (whitespace szűréssel).
        /// </summary>
        private static (ImmutableArray<int> Numbers, ImmutableArray<string> Texts)
        CollectCustomSingleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return (ImmutableArray<int>.Empty, ImmutableArray<string>.Empty);

            var nums = ImmutableArray.CreateBuilder<int>();
            var texts = ImmutableArray.CreateBuilder<string>();

            foreach (var a in list)
            {
                var s = a.Answer?.Trim();
                if (string.IsNullOrEmpty(s)) continue;

                // KIZÁRÓLAG egész számokat tekintünk "numeric" válasznak
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    nums.Add(v);
                else
                    texts.Add(s);
            }

            return (nums.ToImmutable(), texts.ToImmutable());
        }

        /// <summary>
        /// Többválasztós válaszok összegyűjtése (több index egy mezőben, kötőjellel elválasztva).
        /// </summary>
        private static ImmutableArray<int> CollectMultipleChoiceData(
            string id,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var buf = new List<int>(list.Length * 3);
            foreach (var a in list)
            {
                if (string.IsNullOrWhiteSpace(a.Answer)) continue;
                foreach (var token in a.Answer.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    if (int.TryParse(token, out var v))
                        buf.Add(v);
            }
            return [.. buf];
        }

        /// <summary>
        /// Kérdések komponenseinek összeállítása és opcionális kiértékelése.
        /// </summary>
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
                            var data = CollectLikertScaleData(q.Id, index);
                            var ed = new LikertScaleEvaluationData(q.Question, data, q.Description, 1, 5);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.MultinomialSingleChoice:
                        {
                            var data = CollectSingleChoiceData(q.Id, index);
                            var ed = new SingleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], SingleChoice.REGULAR, data, []);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.MultiNomialSingleChoiceOther:
                        {
                            var (nums, texts) = CollectCustomSingleChoiceData(q.Id, index);
                            var ed = new SingleChoiceEvaluationData(
                                q.Question,
                                [.. q.AnswerOptions],
                                SingleChoice.CUSTOM,
                                nums,
                                texts
                            );
                            var src = (evaluate && (HasData(nums) || HasData(texts))) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.MultipleChoice:
                        {
                            var data = CollectMultipleChoiceData(q.Id, index);
                            var ed = new MultipleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], data);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.OpenEnded:
                        {
                            var data = CollectOpenEndedData(q.Id, index);
                            var ed = new OpenEndedEvaluationData(q.Question, data);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
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
        /// </summary>
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
