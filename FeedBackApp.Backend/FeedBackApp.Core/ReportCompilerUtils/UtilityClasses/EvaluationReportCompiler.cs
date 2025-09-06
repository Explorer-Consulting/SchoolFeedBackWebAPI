using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.UtilityClasses
{
    public static class EvaluationReportCompiler
    {
        // arra gondoltam hogy minden 
        private static IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> BuildAnswersIndex(ImmutableArray<QuestionAnswer> answers)
        {
            return answers
                        .GroupBy(a => a.QuestionId)
                        .ToDictionary(g => g.Key, g => g.ToImmutableArray());
        }

        
        private static ImmutableArray<int> CollectLikertScaleData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>(list.Length);
            foreach (var a in list)
                if (int.TryParse(a.Answer, out var v)) b.Add(v);
            return b.MoveToImmutable();
        }

        private static ImmutableArray<int> CollectSingleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<int>(list.Length);
            foreach (var a in list)
                if (int.TryParse(a.Answer, out var v)) b.Add(v);
            return b.MoveToImmutable();
        }

        private static ImmutableArray<string> CollectCustomSingleChoiceData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<string>(list.Length);
            foreach (var a in list)
                if (!string.IsNullOrWhiteSpace(a.Answer)) b.Add(a.Answer);
            return b.MoveToImmutable();
        }

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

        private static ImmutableArray<string> CollectOpenEndedData(string id, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            if (!index.TryGetValue(id, out var list) || list.IsDefaultOrEmpty)
                return [];

            var b = ImmutableArray.CreateBuilder<string>(list.Length);
            foreach (var a in list)
                if (!string.IsNullOrWhiteSpace(a.Answer)) b.Add(a.Answer);
            return b.MoveToImmutable();
        }

        private static ReportDocument CompileRawAnswersData(ReportDocument document, ImmutableArray<QuestionTemplate> questions, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            foreach (var q in questions)
            {
                switch (q.Type)
                {
                    case QuestionType.LikertScaleOneToFive:
                        document.ReportComponents.Add(new LikertScaleEvaluationData(q.Question, CollectLikertScaleData(q.Id, index), q.Description, 1, 5).CompileComponent());
                        break;

                    case QuestionType.MultinomialSingleChoice:
                        document.ReportComponents.Add(new SingleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], SingleChoice.REGULAR, CollectSingleChoiceData(q.Id, index), []).CompileComponent());
                        break;

                    case QuestionType.MultiNomialSingleChoiceOther:
                        document.ReportComponents.Add(new SingleChoiceEvaluationData(q.Question, [], SingleChoice.CUSTOM, [], CollectCustomSingleChoiceData(q.Id, index)).CompileComponent());
                        break;

                    case QuestionType.MultipleChoice:
                        document.ReportComponents.Add(new MultipleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], CollectMultipleChoiceData(q.Id, index)).CompileComponent());
                        break;

                    case QuestionType.OpenEnded:
                        document.ReportComponents.Add(new OpenEndedEvaluationData(q.Question, CollectOpenEndedData(q.Id, index)).CompileComponent());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(q.Type), q.Type, "Ismeretlen kérdéstípus.");
                }
            }
            return document;
        }

        private static ReportDocument CompileDocumentData(ReportDocument document, ImmutableArray<QuestionTemplate> questions, IReadOnlyDictionary<string, ImmutableArray<QuestionAnswer>> index)
        {
            foreach (var q in questions)
            {
                switch (q.Type)
                {
                    case QuestionType.LikertScaleOneToFive:
                        document.ReportComponents.Add(new LikertScaleEvaluationData(q.Question, CollectLikertScaleData(q.Id, index), q.Description, 1, 5).EvaluateData().CompileComponent());
                        break;

                    case QuestionType.MultinomialSingleChoice:
                        document.ReportComponents.Add(new SingleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], SingleChoice.REGULAR, CollectSingleChoiceData(q.Id, index), []).EvaluateData().CompileComponent());
                        break;

                    case QuestionType.MultiNomialSingleChoiceOther:
                        document.ReportComponents.Add(new SingleChoiceEvaluationData(q.Question, [], SingleChoice.CUSTOM, [], CollectCustomSingleChoiceData(q.Id, index)).EvaluateData().CompileComponent());
                        break;

                    case QuestionType.MultipleChoice:
                        document.ReportComponents.Add(new MultipleChoiceEvaluationData(q.Question, [.. q.AnswerOptions], CollectMultipleChoiceData(q.Id, index)).EvaluateData().CompileComponent());
                        break;

                    case QuestionType.OpenEnded:
                        document.ReportComponents.Add(new OpenEndedEvaluationData(q.Question, CollectOpenEndedData(q.Id, index)).EvaluateData().CompileComponent());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(q.Type), q.Type, "Ismeretlen kérdéstípus.");
                }
            }
            return document;
        }

        public static async IAsyncEnumerable<ReportDocument> CompileReports(ImmutableDictionary<Teacher, ImmutableArray<QuestionAnswer>> rawData, ImmutableArray<QuestionTemplate> rawQuestions, ImmutableArray<Administrator> adminEmails)
        {
            ArgumentNullException.ThrowIfNull(rawData);
            ArgumentNullException.ThrowIfNull(rawQuestions);
            ArgumentNullException.ThrowIfNull(adminEmails);

            // 1) Tanáronkénti riport
            foreach (var entry in rawData)
            {
                var teacher = entry.Key;
                var answers = entry.Value;
                var idx = BuildAnswersIndex(answers);

                string fileName = $"{teacher.EmailAddress}_{teacher.SubjectName}_report.pdf";
                var metadata = new ReportMetadata
                {
                    MimeType = "application/pdf",
                    FileName = fileName,
                    Author = "FeedBackApp",
                    BLOB_URI = $"/{fileName}"
                };

                var doc = new TeacherPDFReportDocument(metadata, teacher);
                var compiled = CompileDocumentData(doc, rawQuestions, idx);

                compiled.RenderDocument();
                yield return compiled;
            }

            if (!adminEmails.IsDefaultOrEmpty && adminEmails.Length > 0)
            {
                var allData = rawData.Values.SelectMany(x => x).ToImmutableArray();
                var globalIndex = BuildAnswersIndex(allData);

                // PDF
                var pdfMeta = new ReportMetadata
                {
                    MimeType = "application/pdf",
                    FileName = "global_report.pdf",
                    Author = "FeedBackApp",
                    BLOB_URI = "/global_report.pdf"
                };
                var adminPdf = new AdministratorPDFReportDocument(pdfMeta, adminEmails[0]);
                var compiledPdf = CompileDocumentData(adminPdf, rawQuestions, globalIndex);
                compiledPdf.RenderDocument();
                yield return compiledPdf;

                // Excel
                var excelMeta = new ReportMetadata
                {
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = "global_report.xlsx",
                    Author = "FeedBackApp",
                    BLOB_URI = "/global_report.xlsx"
                };
                var adminExcel = new AdministratorExcelReportDocument(excelMeta, adminEmails[0]);
                var compiledExcel = CompileRawAnswersData(adminExcel, rawQuestions, globalIndex);
                compiledExcel.RenderDocument();
                yield return compiledExcel;
            }
        }
    }
}
