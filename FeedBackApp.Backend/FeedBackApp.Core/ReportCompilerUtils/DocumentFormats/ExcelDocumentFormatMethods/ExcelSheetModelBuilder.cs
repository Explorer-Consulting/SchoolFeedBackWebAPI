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
            var blocksByQuestionID = new Dictionary<string, (QuestionType, List<QuestionBlock> Blocks)>();

            // Local helper: add a block to the given sheet
            void AddBlock(string questionId, QuestionType type, IEnumerable<IEnumerable<string>> headerRows, IEnumerable<IEnumerable<string>> answerRows, IEnumerable<IEnumerable<string>>? optionRows = null)
            {
                if (!blocksByQuestionID.TryGetValue(questionId, out var entry))
                {
                    entry = (type, new List<QuestionBlock>());
                    blocksByQuestionID[questionId] = entry;
                }

                entry.Blocks.Add(new QuestionBlock
                {
                    HeaderRows = headerRows.Select(row => row.Select(cell => cell ?? string.Empty).ToList()).ToList(),
                    OptionRows = optionRows?.Select(row => row.Select(cell => cell ?? string.Empty).ToList()).ToList() ?? [],
                    AnswerRows = answerRows.Select(row => row.Select(cell => cell ?? string.Empty).ToList()).ToList(),
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
                            var headerRows = new List<List<string>>
                            {
                                new List<string> { "Kérdés", l.QuestionStatement }
                            };

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

                            var optionRows = new List<List<string>>
                            {
                                new List<string> { "Opciók:", parsed[0].Index, parsed[0].Text }
                            };


                            for (int i = 1; i < parsed.Count; i++)
                            {
                                optionRows.Add(new List<string> { string.Empty, parsed[i].Index, parsed[i].Text });
                            }

                            var answerRows = new List<List<string>>
                            {
                                new List<string> { "Sorszám", "Választott opciók" }
                            };

                            int j = 1;
                            foreach (var answer in l.Answers)
                            {
                                answerRows.Add(new List<string> { j.ToString(), answer.ToString(CultureInfo.InvariantCulture) });
                                j++;
                            }

                            AddBlock(l.QuestionId, QuestionType.LikertScaleOneToFive, headerRows, answerRows,optionRows);
                            break;
                        }
                        
                    case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                        {
                            var headerRows = new List<List<string>>
                            {
                                new List<string> { "Kérdés", s.QuestionStatement }
                            };

                            var optionRows = new List<List<string>>
                            {
                                new List<string> { "Opciók:","1", s.QuestionOptions[0] }
                            };
        
                            for (int i = 1; i < s.QuestionOptions.Length; i++)
                            {
                                optionRows.Add(new List<string> { string.Empty, $"{i + 1}", s.QuestionOptions[i] });

                            }

                            var answerRows = new List<List<string>>
                            {
                                new List<string> { "Sorszám", "Választott opciók" }
                            };
                            int j = 1;
                            foreach (var answer in s.QuestionOptionAnswers)
                            {
                                answerRows.Add( new List<string> { j.ToString(), answer.ToString(CultureInfo.InvariantCulture) });
                                j++;
                            }

                            AddBlock(s.QuestionId, QuestionType.MultinomialSingleChoice, headerRows, answerRows, optionRows);
                            break;
                        }

                    case SingleChoiceEvaluationData s:
                        {

                            var headerRows = new List<List<string>>
                            {
                                new List<string> { "Kérdés", s.QuestionStatement }
                            };

                            var optionRows = new List<List<string>>
                            {
                                new List<string> { "Opciók:","1", s.QuestionOptions[0] }
                            };

                            for (int i = 1; i < s.QuestionOptions.Length; i++)
                            {
                                optionRows.Add(new List<string> { string.Empty, $"{i + 1}", s.QuestionOptions[i] });
                            }
                
                            int otherIndex = s.QuestionOptions.Length + 1;
                            int nextRow = 1;

                            optionRows.Add(new List<string> { string.Empty, otherIndex.ToString(), "Egyébb" });

                            var answerRows = new List<List<string>>
                            {
                                new List<string> { "Sorszám", "Választott opciók","Szöveg" }
                            };

                            foreach (var answer in s.QuestionOptionAnswers)
                            {
                                answerRows.Add(new List<string> { nextRow.ToString(), answer.ToString(), string.Empty });
                                nextRow++;
                            }

                            foreach (var text in s.QuestionOpenAnswers)
                            {
                                answerRows.Add(new List<string> { nextRow.ToString(), otherIndex.ToString(), text });
                                nextRow++;
                            }

                            AddBlock(s.QuestionId, QuestionType.MultiNomialSingleChoiceOther, headerRows, answerRows, optionRows);
                            break;
                        }
                     
                    case MultipleChoiceEvaluationData m:
                        {

                            var headerRows = new List<List<string>>
                            {
                                new List<string> { "Kérdés", m.QuestionStatement }
                            };

                      
                            var optionRows = new List<List<string>>
                            {
                                new List<string> { "Opciók:", "1", m.AnswerOptions[0] }
                            };

                            for (int i = 1; i < m.AnswerOptions.Length; i++)
                            {
                                optionRows.Add(new List<string> { string.Empty, (i + 1).ToString(), m.AnswerOptions[i] });
                            }

                            var matrixHeader = new List<string> {"Sorszám/Opciók" };
                            for (int i = 1; i <= m.AnswerOptions.Length; i++)
                            {
                                matrixHeader.Add(i.ToString());
                            }

                            var answerRows = new List<List<string>>
                            {
                                matrixHeader 
                            };

                            int respondentNumber = 1;
                            foreach (var respondentAnswers in m.Answers)
                            {
                                var row = new List<string> { respondentNumber.ToString() };

                                for (int optionIndex = 1; optionIndex <= m.AnswerOptions.Length; optionIndex++)
                                {
                                    bool selected = respondentAnswers.Contains(optionIndex);
                                    row.Add(selected ? "1" : string.Empty);
                                }

                                answerRows.Add(row);
                                respondentNumber++;
                            }
                    

                            AddBlock(m.QuestionId, QuestionType.MultipleChoice, headerRows, answerRows, optionRows);
                            break;
                        }
                       
                    case OpenEndedEvaluationData o:
                        {
                            var headerRows = new List<List<string>>
                            {
                                new List<string> { "Kérdés", o.QuestionStatement }
                            };

                            var answerRows = new List<List<string>>
                            {
                                new List<string> { "Sorszám", "Szöveg" }
                            };

                            int i = 1;
                            foreach (var answer in o.Answers)
                            {
                                answerRows.Add( new List<string> { i.ToString(), answer });
                                i++;
                            }
                            AddBlock(o.QuestionId, QuestionType.OpenEnded, headerRows, answerRows);
                            break;
                        }                   
                }
            }

            // Build result
            var result = new List<SheetModel>();

            foreach (var (id, (type, blocks)) in blocksByQuestionID)
            {
                result.Add(new SheetModel
                {
                    Type = type,
                    DisplayName = id,
                    Blocks = blocks
                });
            }


            return result;
        }

    }

}
