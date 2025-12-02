using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;

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
                            AddBlock(QuestionType.LikertScaleOneToFive, new List<string> { "Kérdés", l.QuestionStatement });

                            var parsed = l.ValueMeanings
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .Select(x =>
                                {
                                    var parts = x.Split('=', 2);
                                    return new
                                    {
                                        Index = parts[0].Trim(),
                                        Text = parts[1].Trim()
                                    };
                                })
                                .OrderBy(x => int.Parse(x.Index))
                                .ToList();


                            AddBlock(QuestionType.LikertScaleOneToFive, new List<string> { "Opciók: ", parsed[0].Index, parsed[0].Text });

                            for (int i = 1; i < parsed.Count; i++)
                            {
                                AddBlock(
                                    QuestionType.LikertScaleOneToFive,
                                    new List<string> { "", parsed[i].Index, parsed[i].Text }
                                );
                            }

                            AddBlock(QuestionType.LikertScaleOneToFive, new List<string> { "Sorszám", " Választott opciók" });

                            int j = 1;
                            foreach (var answer in l.Answers)
                            {
                                AddBlock(QuestionType.LikertScaleOneToFive, new List<string> { j.ToString(), answer.ToString(CultureInfo.InvariantCulture) });
                                j++;
                            }
                            break;
                        }

                    case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                        {
                            AddBlock(QuestionType.MultinomialSingleChoice, new List<string> { "Kérdés", s.QuestionStatement });


                            AddBlock(QuestionType.MultinomialSingleChoice, new List<string> { "Opciók: ", "1", s.QuestionOptions[0] });

                            for (int i = 1; i < s.QuestionOptions.Length; i++)
                            {
                                AddBlock(
                                    QuestionType.MultinomialSingleChoice,
                                    new List<string> { string.Empty, $"{i + 1}", s.QuestionOptions[i] }
                                );
                            }



                            AddBlock(QuestionType.MultinomialSingleChoice, new List<string> { "Sorszám", " Választott opciókk" });
                            int j = 1;
                            foreach (var answer in s.QuestionOptionAnswers)
                            {
                                AddBlock(QuestionType.MultinomialSingleChoice, new List<string> { j.ToString(), answer.ToString(CultureInfo.InvariantCulture) });
                                j++;
                            }

                            break;
                        }

                    case SingleChoiceEvaluationData s:
                        {

                            AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> { "Kérdés", s.QuestionStatement });

                            AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> { "Opciók: ", "1", s.QuestionOptions[0] });

                            for (int i = 1; i < s.QuestionOptions.Length; i++)
                            {
                                AddBlock(
                                    QuestionType.MultiNomialSingleChoiceOther,
                                    new List<string> { string.Empty, $"{i + 1}", s.QuestionOptions[i] }
                                );
                            }
                
                            int otherIndex = s.QuestionOptions.Length + 1;
                            int nextRow = 1;

                            AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> { string.Empty, otherIndex.ToString(), "Other" });

                            AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> {"Sorszám","Választott opciók" , "Válaszott szöveg" });



                            foreach (var answer in s.QuestionOptionAnswers)
                            {
                                AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> { nextRow.ToString(), answer.ToString(), string.Empty });
                                nextRow++;
                            }

                            foreach (var text in s.QuestionOpenAnswers)
                            {
                                AddBlock(QuestionType.MultiNomialSingleChoiceOther, new List<string> { nextRow.ToString(), otherIndex.ToString(), text });
                                nextRow++;
                            }

                            break;
                        }

                    case MultipleChoiceEvaluationData m:
                        {

                            AddBlock(QuestionType.MultipleChoice, new List<string> { "Kérdés", m.QuestionStatement });
                            AddBlock(QuestionType.MultipleChoice, new List<string> { "Opciók: ", "1", m.AnswerOptions[0] });

                            for (int i = 1; i < m.AnswerOptions.Length; i++)
                            {
                                AddBlock(
                                    QuestionType.MultipleChoice,
                                    new List<string> { string.Empty, $"{i + 1}", m.AnswerOptions[i] }
                                );
                            }

                            var headerRow = new List<string> { "Opciók" };
                            for (int i = 1; i <= m.AnswerOptions.Length; i++)
                            {
                                headerRow.Add(i.ToString());
                            }
                            AddBlock(QuestionType.MultipleChoice, headerRow);

                            int respondentNumber = 1;
                            foreach (var respondentAnswers in m.Answers)
                            {
                                var row = new List<string> { respondentNumber.ToString() };

                                for (int optionIndex = 1; optionIndex <= m.AnswerOptions.Length; optionIndex++)
                                {
                                    bool selected = respondentAnswers.Contains(optionIndex);
                                    row.Add(selected ? "1" : string.Empty);
                                }

                                AddBlock(QuestionType.MultipleChoice, row);
                                respondentNumber++;
                            }
                        }
                        break;

                    case OpenEndedEvaluationData o:
                        {

                            AddBlock(QuestionType.OpenEnded, new List<string> { "Kérdés", o.QuestionStatement });

                            AddBlock(QuestionType.OpenEnded, new List<string> { "Sorszám", " Szöveg" });

                            int i = 1;
                            foreach (var answer in o.Answers)
                            {
                                AddBlock(QuestionType.OpenEnded, new List<string> { i.ToString(), answer });
                                i++;
                            }
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
                    DisplayName = type.GetDisplayName(),
                    Blocks = blocks
                });
            }


            return result;
        }

    }
}
