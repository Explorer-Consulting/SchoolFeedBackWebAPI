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
    /// Compiler for reports based on collected questionnaire responses.
    /// <para>
    /// Produces teacher-specific and global (administrator) reports in PDF/Excel formats.  
    /// Depending on the question type, the appropriate evaluation models and report components are instantiated.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Main steps:
    /// <list type="number">
    /// <item>Index responses by question ID.</item>
    /// <item>Aggregate data per question type (Likert, single choice, multiple choice, open-ended).</item>
    /// <item>Assemble components and optionally evaluate statistics.</item>
    /// <item>Render documents and stream them as <see cref="IAsyncEnumerable{ReportDocument}"/>.</item>
    /// </list>
    /// </remarks>
    public static class EvaluationReportCompiler
    {
        // --- Helpers ---
        private static bool HasData<T>(ImmutableArray<T> data) => !data.IsDefault && data.Length > 0;

        /// <summary>
        /// Indexes responses by question ID (<see cref="QuestionAnswer.QuestionId"/>).
        /// </summary>
        private static IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> BuildAnswersIndex(ImmutableArray<QuestionAnswer> answers) => answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToImmutableArray());

        /// <summary>
        /// Collects Likert-scale responses (integers) for a given question.
        /// </summary>
        private static ImmutableArray<int> CollectLikertScaleData(
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
        /// Collects single-choice responses (index-based).
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
        /// Collects open-ended (textual) responses (filters out whitespace).
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
        /// Collects custom free-text “Other” responses (filters out whitespace).  
        /// Separates numeric answers from non-numeric text.
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

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    nums.Add(v);
                else
                    texts.Add(s);
            }

            return (nums.ToImmutable(), texts.ToImmutable());
        }

        /// <summary>
        /// Collects multiple-choice responses (multiple indices encoded in a single field, separated by hyphens).
        /// </summary>
        private static ImmutableArray<ImmutableArray<int>> CollectMultipleChoiceData(
            string id,
            IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var builder = ImmutableArray.CreateBuilder<ImmutableArray<int>>();

            foreach (var a in list)
            {
                if (string.IsNullOrWhiteSpace(a.Answer)) continue;

                var options = ImmutableArray.CreateBuilder<int>();
                foreach (var token in a.Answer.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    if (int.TryParse(token, out var v))
                        options.Add(v);

                builder.Add(options.ToImmutable());
            }
            return [.. builder];
        }

        /// <summary>
        /// Compiles components for questions and optionally evaluates statistics.
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
                            var ed = new LikertScaleEvaluationData(q.Id, q.Question, data, q.Description, 1, 5);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.MultinomialSingleChoice:
                        {
                            var data = CollectSingleChoiceData(q.Id, index);
                            var ed = new SingleChoiceEvaluationData(q.Id, q.Question, [.. q.AnswerOptions], SingleChoice.REGULAR, data, []);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.MultiNomialSingleChoiceOther:
                        {
                            var (nums, texts) = CollectCustomSingleChoiceData(q.Id, index);
                            var ed = new SingleChoiceEvaluationData(
                                q.Id,
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
                            var ed = new MultipleChoiceEvaluationData(q.Id, q.Question, [.. q.AnswerOptions], data);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    case QuestionType.OpenEnded:
                        {
                            var data = CollectOpenEndedData(q.Id, index);
                            var ed = new OpenEndedEvaluationData(q.Id, q.Question, data);
                            var src = (evaluate && HasData(data)) ? ed.EvaluateData() : ed;
                            document.ReportComponents.Add(src.CompileComponent());
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(q.Type), q.Type, "Unknown question type.");
                }
            }

            return document;
        }

        /// <summary>
        /// Full report compilation streamed as an async sequence:
        /// <list type="bullet">
        /// <item>Teacher-specific PDFs</item>
        /// <item>Global PDF</item>
        /// <item>Global Excel (raw data)</item>
        /// </list>
        /// </summary>
        public static async IAsyncEnumerable<ReportDocument> CompileReports(
            ImmutableDictionary<Teacher, ImmutableArray<QuestionAnswer>> rawData,
            ImmutableArray<QuestionTemplate> rawQuestions,
            string surveyId)
        {
            ArgumentNullException.ThrowIfNull(rawData);
            ArgumentNullException.ThrowIfNull(rawQuestions);
            ArgumentException.ThrowIfNullOrEmpty(surveyId);

            // 1) Teacher-specific PDFs
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

            // 2/a) Global PDF
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

            // 2/b) Global Excel (raw data)
            {
                const string fileName = "global_report.xlsx";
                var metadata = new ReportMetadata
                {
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = fileName,
                    Author = "Explorer Consulting",
                    BLOB_URI = string.Empty
                };

                var adminExcel = new ExcelReportDocument(metadata);
                var allData = rawData.Values.SelectMany(x => x).ToImmutableArray();
                ReportDocument compiledExcel;
                Task<byte[]> renderTask;
                CreateRenderOfDocument(rawQuestions, adminExcel, allData, out compiledExcel, out renderTask);
                await renderTask;
                yield return compiledExcel;
            }
        }

        public static void CreateRenderOfDocument(ImmutableArray<QuestionTemplate> rawQuestions, ExcelReportDocument adminExcel, ImmutableArray<QuestionAnswer> allData, out ReportDocument compiledExcel, out Task<byte[]> renderTask)
        {
            var globalIndex = BuildAnswersIndex(allData);
            compiledExcel = CompileQuestions(adminExcel, rawQuestions, globalIndex, evaluate: false);
            renderTask = compiledExcel.RenderDocument();
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
