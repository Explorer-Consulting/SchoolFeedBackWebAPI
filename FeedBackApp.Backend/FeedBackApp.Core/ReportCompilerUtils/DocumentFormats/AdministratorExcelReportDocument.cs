using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentUtils;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Document class that generates the Administrator Excel report.
    /// Based on the DataSource of the report components found in <see cref="ReportComponent"/>,
    /// it creates separate worksheets per type (Likert, SingleChoice, SingleChoice+Other, MultipleChoice, OpenEnded),
    /// with automatic column widths, base styles, and a frozen header row.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>DataSource</c> property of each component is read via reflection, and the switch
    /// cases below convert it into normalized rows based on the type. Each question forms a “block”:
    /// a main row (Main) and optionally an options row (Opts). Numeric values are written as Number cells,
    /// text values as InlineString cells.
    /// </para>
    /// <para>
    /// If <see cref="ReportComponents"/> is empty, an “Empty” sheet is created as an indicator.
    /// The generated document is built in memory, and <see cref="RenderDocument"/> returns a byte array.
    /// </para>
    /// </remarks>
    public sealed class AdministratorExcelReportDocument(ReportMetadata metadata, Recipient? recipient = null)
        : ReportDocument(metadata, recipient)
    {
        /// <summary>
        /// Builds the complete .xlsx file: workbook, stylesheet, sheets, rows, and columns.
        /// </summary>
        /// <returns>The generated Excel document bytes.</returns>
        /// <remarks>
        /// Steps:
        /// 1) Create Workbook and Stylesheet.
        /// 2) Traverse components and group blocks by sheet type.
        /// 3) Record maximum counts of answers/options for within-sheet normalization.
        /// 4) Create worksheets with consistent row widths.
        /// 5) Save the workbook and return the byte[].
        /// </remarks>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Workbook and stylesheet
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                var styles = wbPart.AddNewPart<WorkbookStylesPart>();
                styles.Stylesheet = ExcelBuildStylesheet.BuildStylesheet();
                styles.Stylesheet.Save();

                var sheets = wbPart.Workbook.AppendChild(new Sheets());

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
                foreach (var comp in ReportComponents)
                {
                    var ds = comp.GetType().GetProperty("DataSource")?.GetValue(comp);
                    if (ds is null) continue;

                    switch (ds)
                    {
                        case LikertScaleEvaluationData l:
                            {
                                var main = new List<string> { l.QuestionStatement };
                                main.AddRange(l.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                                main.Add(l.ValueMeanings ?? string.Empty);

                                AddBlock("Likert-skála", main);
                                HelperFormatUtils.UpdateMax(maxAnsBySheet, "Likert-skála", l.Answers.Length);
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
                                HelperFormatUtils.UpdateMax(maxAnsBySheet, "Egyválasztós", s.QuestionOptionAnswers.Length);
                                HelperFormatUtils.UpdateMax(maxOptsBySheet, "Egyválasztós", s.QuestionOptions.Length);
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
                                    HelperFormatUtils.UpdateMax(maxOptsBySheet, "Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + answer
                                }

                                // Predefined options in separate rows
                                if (!s.QuestionOptions.IsDefaultOrEmpty && s.QuestionOptions.Length > 0)
                                {
                                    int idx = 1;
                                    foreach (var opt in s.QuestionOptions)
                                    {
                                        blocks.Add((new List<string>(), new List<string> { "Opció", $"{idx++} = {opt}" }));
                                    }
                                    HelperFormatUtils.UpdateMax(maxOptsBySheet, "Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + option
                                }

                                // Add to the sheet
                                if (!blocksBySheet.TryGetValue("Egyválasztós + Nyílt végű kérdés", out var list))
                                    blocksBySheet["Egyválasztós + Nyílt végű kérdés"] = list = new();
                                list.AddRange(blocks);

                                // Max numeric columns
                                HelperFormatUtils.UpdateMax(maxAnsBySheet, "Egyválasztós + Nyílt végű kérdés", s.QuestionOptionAnswers.Length);

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
                                HelperFormatUtils.UpdateMax(maxAnsBySheet, "Többválasztós", m.Answers.Length);
                                HelperFormatUtils.UpdateMax(maxOptsBySheet, "Többválasztós", m.AnswerOptions.Length);
                                break;
                            }

                        case OpenEndedEvaluationData o:
                            {
                                var main = new List<string> { o.QuestionStatement };
                                main.AddRange(o.Answers);

                                AddBlock("Nyílt végű", main);
                                HelperFormatUtils.UpdateMax(maxAnsBySheet, "Nyílt végű", o.Answers.Length);
                                break;
                            }
                    }
                }

                // If no data, create an "Empty" sheet
                if (blocksBySheet.Count == 0)
                {
                    var emptyBlocks = new List<(List<string> Main, List<string> Opts)>
                    {
                        (new List<string>{ "—" }, new List<string>())
                    };

                    ExcelCreateSheet.CreateSheet(
                        wbPart, sheets, "Üres",
                        header: ["Kérdés"],
                        blocks: emptyBlocks,
                        explicitSheetId: null,
                        maxAns: 0, maxOpts: 0
                    );
                }
                else
                {
                    // We have data: create a separate worksheet for each sheet type
                    uint sheetId = 1;
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (rawName, blocks) in blocksBySheet)
                    {
                        var sheetName = HelperFormatUtils.MakeSafeSheetName(rawName, usedNames);

                        var maxAns = maxAnsBySheet.TryGetValue(rawName, out var ma) ? ma : 0;
                        var maxOpts = maxOptsBySheet.TryGetValue(rawName, out var mo) ? mo : 0;

                        // Header for the main table
                        var header = new List<string> { "Kérdés" };
                        if (sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase))
                        {
                            for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                            header.Add("Értékek jelentése");
                        }
                        else
                        {
                            for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                        }

                        // Total width: max(main, options)
                        var mainCols = 1 + maxAns + (sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
                        var optionCols = 1 + maxOpts;
                        var totalCols = Math.Max(mainCols, optionCols);
                        while (header.Count < totalCols) header.Add(string.Empty);

                        // Normalize blocks to totalCols width
                        var normalized = new List<(List<string> Main, List<string> Opts)>(blocks.Count);
                        foreach (var blk in blocks)
                        {
                            var m = new List<string>(blk.Main);
                            var o = new List<string>(blk.Opts ?? new List<string>());

                            while (m.Count < mainCols) m.Add(string.Empty);
                            while (o.Count < optionCols) o.Add(string.Empty);
                            while (m.Count < totalCols) m.Add(string.Empty);
                            while (o.Count < totalCols) o.Add(string.Empty);

                            normalized.Add((m, o));
                        }

                        ExcelCreateSheet.CreateSheet(
                            wbPart, sheets, sheetName,
                            header, normalized, sheetId++,
                            maxAns, maxOpts
                        );
                    }
                }

                wbPart.Workbook.Save();
            }

            Data = ms.ToArray();
            return Task.FromResult(Data);
        }
      
    }
}
