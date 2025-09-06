using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.UtilityClasses
{
    /*
     Itt ezt meg ki kell egesziteni azzal, hogy egy tanar lassa, hogy a sajat tantargyabol o hany ertekelest kapott es a tobbi hanyat,
     egyebb statisztikai mutatokkal kell kiegesziteni
     */
    public static class EvaluationReportCompiler
    {
        private static ImmutableArray<int> CollectLikertScaleData(string id, ImmutableArray<QuestionAnswer> answers)
        {
            ImmutableArray<int> data = [.. answers
                .Where(a => a.QuestionId == id)
                .Select(a => int.Parse(a.Answer))];
            return data;
        }
        private static ImmutableArray<int> CollectSingleChoiceData(string id, ImmutableArray<QuestionAnswer> answers)
        {
            ImmutableArray<int> data = [.. answers
                .Where(a => a.QuestionId == id)
                .Select(a => int.Parse(a.Answer))];
            return data;
        }
        private static ImmutableArray<string> CollectCustomSingleChoiceData(string id, ImmutableArray<QuestionAnswer> answers)
        {
            ImmutableArray<string> data = [.. answers
                .Where(a => a.QuestionId == id)
                .Select(a => a.Answer)];
            return data;
        }
        private static ImmutableArray<int> CollectMultipleChoiceData(string id, ImmutableArray<QuestionAnswer> answers)
        {
            var data = answers
                .Where(a => a.QuestionId == id && !string.IsNullOrWhiteSpace(a.Answer))
                .SelectMany(a => a.Answer
                    .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(int.Parse))
                .ToImmutableArray();

            return data;
        }
        private static ImmutableArray<string> CollectOpenEndedData(string id, ImmutableArray<QuestionAnswer> answers)
        {
            ImmutableArray<string> data = [.. answers
                .Where(a => a.QuestionId == id)
                .Select(a => a.Answer)];
            return data;
        }
        private static ReportDocument CompileRawAnswersData(ReportDocument document, ImmutableArray<QuestionTemplate> questions, ImmutableArray<QuestionAnswer> answers)
        {
            foreach (var x in questions)
            {
                // itt majd leroviditem
                switch (x.Type)
                {
                    case QuestionType.LikertScaleOneToFive: // itt gyujtjuk ossze a LikertSkalas kerdesekhez szukseges adatokat
                        string likertID = x.Id;
                        string likertStatement = x.Question;
                        // itt lesz egy Likert Decription property, amelyel majd frissitjuk amit kell.
                        string likertMeanings = null;
                        var likertData = CollectLikertScaleData(likertID, answers);
                        LikertScaleEvaluationData a = new(likertStatement, likertData, likertMeanings, 1, 5);
                        document.ReportComponents.Add(a.CompileComponent());
                        //ez eccer kesz.
                        break;
                    case QuestionType.MultinomialSingleChoice: // ez a sima 
                        string multiID = x.Id;
                        string multiStatement = x.Question;
                        ImmutableArray<string> answerOptions = [.. x.AnswerOptions];
                        var singleChoiceData = CollectSingleChoiceData(multiID, answers);
                        SingleChoiceEvaluationData b = new(multiStatement, answerOptions, SingleChoice.REGULAR, singleChoiceData, []);
                        document.ReportComponents.Add(b.CompileComponent());
                        break;
                    case QuestionType.MultiNomialSingleChoiceOther: // ez a szabad feleletes
                        string multiOtherID = x.Id;
                        string multiOtherStatement = x.Question;
                        var singleChoiceOtherData = CollectCustomSingleChoiceData(multiOtherID, answers);
                        SingleChoiceEvaluationData c = new(multiOtherStatement, [], SingleChoice.CUSTOM, [], singleChoiceOtherData);
                        document.ReportComponents.Add(c.CompileComponent());
                        break;
                    case QuestionType.MultipleChoice:
                        string multiChoiceID = x.Id;
                        string multiChoiceStatement = x.Question;
                        ImmutableArray<string> multiChoiceOptions = [.. x.AnswerOptions];
                        var multiChoiceData = CollectMultipleChoiceData(multiChoiceID, answers);
                        MultipleChoiceEvaluationData d = new(multiChoiceStatement, multiChoiceOptions, multiChoiceData);
                        document.ReportComponents.Add(d.CompileComponent());
                        break;
                    case QuestionType.OpenEnded:
                        string openEndedID = x.Id;
                        string openEndedStatement = x.Question;
                        var openEndedData = CollectOpenEndedData(openEndedID, answers);
                        OpenEndedEvaluationData e = new(openEndedStatement, openEndedData);
                        document.ReportComponents.Add(e.CompileComponent());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x.Type), x.Type, "Ismeretlen kérdéstípus.");
                }
            }
            return document;
        }
        // egy adott tanarhoz tartozo osszes kerdest dolgozzuk fel
        private static ReportDocument CompileDocumentData(ReportDocument document, ImmutableArray<QuestionTemplate> questions, ImmutableArray<QuestionAnswer> answers)
        {
            foreach(var x in questions)
            {
                switch (x.Type)
                {
                    case QuestionType.LikertScaleOneToFive: // itt gyujtjuk ossze a LikertSkalas kerdesekhez szukseges adatokat
                        string likertID = x.Id;
                        string likertStatement = x.Question;
                        // itt is szinten ugyanazt
                        string likertMeanings = null;
                        var likertData = CollectLikertScaleData(likertID, answers);
                        LikertScaleEvaluationData a = new(likertStatement, likertData, likertMeanings, 1, 5);
                        var d1 = a.EvaluateData();
                        document.ReportComponents.Add(d1.CompileComponent());
                        // ez eccer kesz.
                        break;
                    case QuestionType.MultinomialSingleChoice: // ez a sima 
                        string multiID = x.Id;
                        string multiStatement = x.Question;
                        ImmutableArray<string> answerOptions = [.. x.AnswerOptions];
                        var singleChoiceData = CollectSingleChoiceData(multiID, answers);
                        SingleChoiceEvaluationData b = new(multiStatement, answerOptions, SingleChoice.REGULAR, singleChoiceData, []);
                        var d2 = b.EvaluateData();
                        document.ReportComponents.Add(d2.CompileComponent());
                        break;
                    case QuestionType.MultiNomialSingleChoiceOther: // ez a szabad feleletes
                        string multiOtherID = x.Id;
                        string multiOtherStatement = x.Question;
                        var singleChoiceOtherData = CollectCustomSingleChoiceData(multiOtherID, answers);
                        SingleChoiceEvaluationData c = new(multiOtherStatement, [], SingleChoice.CUSTOM, [], singleChoiceOtherData);
                        var d3 = c.EvaluateData();
                        document.ReportComponents.Add(d3.CompileComponent());
                        break;
                    case QuestionType.MultipleChoice:
                        string multiChoiceID = x.Id;
                        string multiChoiceStatement = x.Question;
                        ImmutableArray<string> multiChoiceOptions = [.. x.AnswerOptions];
                        var multiChoiceData = CollectMultipleChoiceData(multiChoiceID, answers);
                        MultipleChoiceEvaluationData d = new(multiChoiceStatement, multiChoiceOptions, multiChoiceData);
                        var d4 = d.EvaluateData();
                        document.ReportComponents.Add(d4.CompileComponent());
                        break;
                    case QuestionType.OpenEnded:
                        string openEndedID = x.Id;
                        string openEndedStatement = x.Question;
                        var openEndedData = CollectOpenEndedData(openEndedID, answers);
                        OpenEndedEvaluationData e = new(openEndedStatement, openEndedData);
                        var d5 = e.EvaluateData();
                        document.ReportComponents.Add(d5.CompileComponent());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x.Type), x.Type, "Ismeretlen kérdéstípus.");
                }
            }
            return document;
        }
        public static async IAsyncEnumerable<ReportDocument> CompileReports(ImmutableDictionary<Teacher, ImmutableArray<QuestionAnswer>> rawData, ImmutableArray<QuestionTemplate> rawQuestions, ImmutableArray<Administrator> adminEmails)
        {
            ArgumentNullException.ThrowIfNull(rawData);
            ArgumentNullException.ThrowIfNull(rawQuestions);
            ArgumentNullException.ThrowIfNull(adminEmails);

            // interating through the the teachers
            foreach(var recipientData in rawData)
            {
                Teacher recipient = recipientData.Key; // the actual teacher
                ImmutableArray<QuestionAnswer> answers = recipientData.Value; // all answers related to the given teacher

                string fileName = $"{recipient.EmailAddress}_{recipient.SubjectName}_report.pdf"; //the document's filename
                ReportMetadata metadata = new() // metadata for the given document → may be customized later
                {
                    MimeType = "application/pdf",
                    FileName = fileName,
                    Author = "Valaki",
                    BLOB_URI = $"/{fileName}"
                };
                TeacherPDFReportDocument reportDocument = new(metadata, recipient); // creating a new document prototype.

                var doc = CompileDocumentData(reportDocument, rawQuestions, answers); // providing and processing data for a given ReportDocument object
                await doc.RenderDocument(); // rendering the actual PDF report for teacher
                yield return doc; // returning the document
            }
            foreach(var recipientData in adminEmails)
            {
                Administrator recipient = recipientData;
                ImmutableArray<QuestionAnswer> allData = [.. rawData.Values.SelectMany(answers => answers)];

                #region itt generaljuk az adminok szamara a PDF-eket
                string pdfFileName = $"{recipient.EmailAddress}_global_report.pdf";
                ReportMetadata metadataPDF = new()
                {
                    MimeType = "application/pdf",
                    FileName = pdfFileName,
                    Author = "Valaki",
                    BLOB_URI = $"/{pdfFileName}"
                };
                AdministratorPDFReportDocument adminReportPDF = new(metadataPDF, recipient);
                var reportDocument = CompileDocumentData(adminReportPDF, rawQuestions, allData);
                await reportDocument.RenderDocument();
                #endregion
                // itt majd lesz egy await ami kigeneralja a vegleges dokumentumot, addig is...
                yield return reportDocument;

                #region itt generaljuk a adminok szamara a EXCEL-eket
                string excelFileName = $"{recipient.EmailAddress}_global_report.xlsx";
                ReportMetadata metadataEXCEL = new()
                {
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = pdfFileName,
                    Author = "Valaki",
                    BLOB_URI = $"/{pdfFileName}"
                };
                AdministratorExcelReportDocument adminReportExcel = new(metadataEXCEL, recipient);
                var reportDocumentExcel = CompileRawAnswersData(adminReportExcel, rawQuestions, allData);
                await reportDocumentExcel.RenderDocument();
                #endregion
                //itt maj dlesz egy await ami kigenerlaja a vegleges dokumentumot, addig is...
                yield return reportDocumentExcel;
            }

        }
    }
}
