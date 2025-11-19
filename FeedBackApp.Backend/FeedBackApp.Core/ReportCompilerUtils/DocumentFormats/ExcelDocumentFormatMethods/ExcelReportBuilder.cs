using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.model;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods
{
    internal static class ExcelReportBuilder
    {
        public static IReadOnlyList<SheetModel> BuildSheets(
        IEnumerable<IReportComponent> components)
        {

            // Sheet type -> blocks (Main: question + answers, Opts: options row)
            var blocksBySheet = new Dictionary<string, List<(List<string> Main, List<string> Opts)>>(StringComparer.OrdinalIgnoreCase);
            var maxAnsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var maxOptsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Local helper: add a block to the given sheet
            void AddBlock(string sheet, IEnumerable<string> main, IEnumerable<string>? opts = null)
            {
                if (!blocksBySheet.TryGetValue(sheet, out var list))
                    blocksBySheet[sheet] = list = new();
                list.Add(
                    (main.Select(x => x ?? string.Empty).ToList(),
                     opts is null ? [] : opts.Select(x => x ?? string.Empty).ToList())
                );
            }

            // Traverse components, inspect DataSource, and create blocks
            foreach (var comp in components)
            {
                var ds = comp.DataSourceUntyped;
                if (ds is null) continue;

                switch (ds)
                {
                    case LikertScaleEvaluationData l:
                        {
                            var main = new List<string> { l.QuestionStatement };
                            main.AddRange(l.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                            main.Add(l.ValueMeanings ?? string.Empty);

                            AddBlock("Likert-skála", main);
                            maxAnsBySheet.UpdateMax("Likert-skála", l.Answers.Length);
                            break;
                        }

                    case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                        {
                            var main = new List<string> { s.QuestionStatement };
                            main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            var opts = new List<string> { "Opciók" };
                            for (int i = 0; i < s.QuestionOptions.Length; i++)
                                opts.Add($"{i + 1} = {s.QuestionOptions[i]}");

                            AddBlock("Egyválasztós", main, opts);
                            maxAnsBySheet.UpdateMax("Egyválasztós", s.QuestionOptionAnswers.Length);
                            maxOptsBySheet.UpdateMax("Egyválasztós", s.QuestionOptions.Length);
                            break;
                        }

                    case SingleChoiceEvaluationData s:
                        {
                            var main = new List<string> { s.QuestionStatement };
                            if (s.QuestionOptionAnswers.Length > 0)
                                main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            // Primary block: always ensure we have a Main row
                            var blocks = new List<(List<string> Main, List<string> Opts)>
                                {
                                    (main, new List<string>()) // Opts is empty here
                                };

                            // Text answers in separate rows
                            if (!s.QuestionOpenAnswers.IsDefaultOrEmpty && s.QuestionOpenAnswers.Length > 0)
                            {
                                foreach (var ans in s.QuestionOpenAnswers)
                                {
                                    blocks.Add((new List<string>(), new List<string> { "Szöveges válasz", ans }));
                                }
                                maxOptsBySheet.UpdateMax("Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + answer
                            }

                            // Predefined options in separate rows
                            if (!s.QuestionOptions.IsDefaultOrEmpty && s.QuestionOptions.Length > 0)
                            {
                                int idx = 1;
                                foreach (var opt in s.QuestionOptions)
                                {
                                    blocks.Add((new List<string>(), new List<string> { "Opció", $"{idx++} = {opt}" }));
                                }
                                maxOptsBySheet.UpdateMax("Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + option
                            }

                            // Add to the sheet
                            if (!blocksBySheet.TryGetValue("Egyválasztós + Nyílt végű kérdés", out var list))
                                blocksBySheet["Egyválasztós + Nyílt végű kérdés"] = list = new();
                            list.AddRange(blocks);

                            // Max numeric columns
                            maxAnsBySheet.UpdateMax("Egyválasztós + Nyílt végű kérdés", s.QuestionOptionAnswers.Length);

                            break;
                        }

                    case MultipleChoiceEvaluationData m:
                        {
                            var main = new List<string> { m.QuestionStatement };
                            main.AddRange(m.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            var opts = new List<string> { "Opciók" };
                            for (int i = 0; i < m.AnswerOptions.Length; i++)
                                opts.Add($"{i + 1} = {m.AnswerOptions[i]}");

                            AddBlock("Többválasztós", main, opts);
                            maxAnsBySheet.UpdateMax("Többválasztós", m.Answers.Length);
                            maxOptsBySheet.UpdateMax("Többválasztós", m.AnswerOptions.Length);
                            break;
                        }

                    case OpenEndedEvaluationData o:
                        {
                            var main = new List<string> { o.QuestionStatement };
                            main.AddRange(o.Answers);

                            AddBlock("Nyílt végű", main);
                            maxAnsBySheet.UpdateMax("Nyílt végű", o.Answers.Length);
                            break;
                        }
                }
            }

            var result = new List<SheetModel>();

            foreach (var (rawName, blocks) in blocksBySheet)
            {
                result.Add(new SheetModel
                {
                    RawName = rawName,
                    Blocks = blocks,
                    MaxAns = maxAnsBySheet.TryGetValue(rawName, out var ma) ? ma : 0,
                    MaxOpts = maxOptsBySheet.TryGetValue(rawName, out var mo) ? mo : 0
                });
            }

            return result;
        }

    }
}
