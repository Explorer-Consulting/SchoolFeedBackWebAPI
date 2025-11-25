using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods
{
    /// <summary>
    /// Builds domain sheet models from report components.
    /// <para>
    /// Groups questions by type (<see cref="QuestionType"/>) and creates 
    /// a <see cref="SheetModel"/> for each type containing the question blocks.
    /// </para>
    /// </summary>
    /// <param name="components">The report components containing evaluated question data.</param>
    /// <returns>A list of sheet models, one per question type.</returns>
    internal static class ExcelSheetModelBuilder
    {
        public static IReadOnlyList<SheetModel> BuildSheetsModelsFromComponents(
        IEnumerable<IReportComponent> components)
        {

            // Sheet type -> blocks (Main: question + answers, Opts: options row)
            var blocksBySheet = new Dictionary<QuestionType, List<QuestionBlock>>();

            // Local helper: add a block to the given sheet
            void AddBlock(QuestionType type, IEnumerable<string> mainRow, IEnumerable<string>? optionsRow = null)
            {
                if (!blocksBySheet.TryGetValue(type, out var list))
                    blocksBySheet[type] = list = [];
                list.Add(new QuestionBlock
                {
                    MainRow = mainRow.Select(x => x ?? string.Empty).ToList(),
                    OptionsRow = optionsRow?.Select(x => x ?? string.Empty).ToList() ?? []
                });
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

                            AddBlock(QuestionType.LikertScaleOneToFive, main);
                            break;
                        }

                    case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                        {
                            var main = new List<string> { s.QuestionStatement };
                            main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            var opts = new List<string> { "Opciók" };
                            for (int i = 0; i < s.QuestionOptions.Length; i++)
                                opts.Add($"{i + 1} = {s.QuestionOptions[i]}");

                            AddBlock(QuestionType.MultinomialSingleChoice, main, opts);
                            break;
                        }

                    case SingleChoiceEvaluationData s:
                        {
                            var main = new List<string> { s.QuestionStatement };
                            if (s.QuestionOptionAnswers.Length > 0)
                                main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            // First block: main row
                            AddBlock(QuestionType.MultiNomialSingleChoiceOther, main);


                            // Text answers in separate rows
                            if (!s.QuestionOpenAnswers.IsDefaultOrEmpty && s.QuestionOpenAnswers.Length > 0)
                            {
                                foreach (var ans in s.QuestionOpenAnswers)
                                {
                                    var textOpts = new List<string> { "Szöveges válasz", ans };
                                    AddBlock(QuestionType.MultiNomialSingleChoiceOther, [], textOpts);
                                }
                            }


                            // Predefined options in separate rows
                            if (!s.QuestionOptions.IsDefaultOrEmpty && s.QuestionOptions.Length > 0)
                            {
                                int idx = 1;
                                foreach (var opt in s.QuestionOptions)
                                {
                                    var optionOpts = new List<string> { "Opció", $"{idx++} = {opt}" };
                                    AddBlock(QuestionType.MultiNomialSingleChoiceOther, [], optionOpts);
                                }
                            }
                            break;
                        }

                    case MultipleChoiceEvaluationData m:
                        {
                            var main = new List<string> { m.QuestionStatement };
                            main.AddRange(m.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                            var opts = new List<string> { "Opciók" };
                            for (int i = 0; i < m.AnswerOptions.Length; i++)
                                opts.Add($"{i + 1} = {m.AnswerOptions[i]}");

                            AddBlock(QuestionType.MultipleChoice, main, opts);
                            break;
                        }

                    case OpenEndedEvaluationData o:
                        {
                            var main = new List<string> { o.QuestionStatement };
                            main.AddRange(o.Answers);

                            AddBlock(QuestionType.OpenEnded, main);
                            break;
                        }
                }
            }

            // Build result
            var result = new List<SheetModel>();

            foreach (var (type, blocks) in blocksBySheet)
            {
                result.Add(new SheetModel
                {
                    Type = type,
                    DisplayName = GetDisplayName(type),
                    Blocks = blocks
                });
            }


            return result;
        }

        /// <summary>
        /// Gets the display name for a given question type.
        /// </summary>
        private static string GetDisplayName(QuestionType type) => type switch
        {
            QuestionType.LikertScaleOneToFive => "Likert-skála",
            QuestionType.MultinomialSingleChoice => "Egyválasztós",
            QuestionType.MultiNomialSingleChoiceOther => "Egyválasztós + Nyílt végű kérdés",
            QuestionType.MultipleChoice => "Többválasztós",
            QuestionType.OpenEnded => "Nyílt végű",
            _ => "Ismeretlen"
        };
    }
}
