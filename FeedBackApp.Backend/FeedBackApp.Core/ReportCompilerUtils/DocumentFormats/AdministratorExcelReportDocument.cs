using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
                styles.Stylesheet = ExcelBuildStylesheetUtils.BuildStylesheet();
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
                                UpdateMax(maxAnsBySheet, "Likert-skála", l.Answers.Length);
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
                                UpdateMax(maxAnsBySheet, "Egyválasztós", s.QuestionOptionAnswers.Length);
                                UpdateMax(maxOptsBySheet, "Egyválasztós", s.QuestionOptions.Length);
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
                                    UpdateMax(maxOptsBySheet, "Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + answer
                                }

                                // Predefined options in separate rows
                                if (!s.QuestionOptions.IsDefaultOrEmpty && s.QuestionOptions.Length > 0)
                                {
                                    int idx = 1;
                                    foreach (var opt in s.QuestionOptions)
                                    {
                                        blocks.Add((new List<string>(), new List<string> { "Opció", $"{idx++} = {opt}" }));
                                    }
                                    UpdateMax(maxOptsBySheet, "Egyválasztós + Nyílt végű kérdés", 2); // 2 columns: label + option
                                }

                                // Add to the sheet
                                if (!blocksBySheet.TryGetValue("Egyválasztós + Nyílt végű kérdés", out var list))
                                    blocksBySheet["Egyválasztós + Nyílt végű kérdés"] = list = new();
                                list.AddRange(blocks);

                                // Max numeric columns
                                UpdateMax(maxAnsBySheet, "Egyválasztós + Nyílt végű kérdés", s.QuestionOptionAnswers.Length);

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
                                UpdateMax(maxAnsBySheet, "Többválasztós", m.Answers.Length);
                                UpdateMax(maxOptsBySheet, "Többválasztós", m.AnswerOptions.Length);
                                break;
                            }

                        case OpenEndedEvaluationData o:
                            {
                                var main = new List<string> { o.QuestionStatement };
                                main.AddRange(o.Answers);

                                AddBlock("Nyílt végű", main);
                                UpdateMax(maxAnsBySheet, "Nyílt végű", o.Answers.Length);
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

                    CreateSheet(
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
                        var sheetName = MakeSafeSheetName(rawName, usedNames);

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

                        CreateSheet(
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

        /// <summary>
        /// Creates a worksheet and populates it with the specified header and data rows (Main + Opts).
        /// </summary>
        /// <param name="wbPart">The workbook part.</param>
        /// <param name="sheets">The workbook’s sheet collection.</param>
        /// <param name="sheetName">The worksheet name (Excel-compatible, see <see cref="MakeSafeSheetName"/>).</param>
        /// <param name="header">The main table header (first row).</param>
        /// <param name="blocks">The normalized blocks (Main row and optional Opts row).</param>
        /// <param name="explicitSheetId">Optional sheet ID.</param>
        /// <param name="maxAns">The maximum number of answer columns on this sheet.</param>
        /// <param name="maxOpts">The maximum number of option columns on this sheet.</param>
        /// <remarks>
        /// - The top row is frozen (A2) so the header remains visible while scrolling.
        /// - Numeric columns (Likert/SingleChoice/MultipleChoice answers) are right-aligned Number cells.
        /// - The "Opts" row is written only if it contains the "Options" label and at least one option.
        /// - Style indexes:
        ///   1 = header (bold, gray background),
        ///   2 = text data,
        ///   3 = options row (italic, light background),
        ///   4 = numeric data (bluish background).
        /// </remarks>
        private static void CreateSheet(
            WorkbookPart wbPart, Sheets sheets,
            string sheetName,
            IReadOnlyList<string> header,
            IReadOnlyList<(List<string> Main, List<string> Opts)> blocks,
            uint? explicitSheetId,
            int maxAns,
            int maxOpts)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();

            // Column width estimation based on content
            var cols = BuildAutoColumns(header, blocks, sheetName, maxAns);

            // Freeze header (Pane)
            var views = new SheetViews(new SheetView
            {
                WorkbookViewId = 0,
                Pane = new Pane { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen }
            });

            wsPart.Worksheet = new Worksheet(views, cols, sheetData);

            // Register sheet
            var sheet = new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = explicitSheetId ?? (uint)(sheets.Count() + 1),
                Name = sheetName
            };
            sheets.Append(sheet);

            // Header (style 1)
            var headerRow = new Row();
            foreach (var text in header)
                headerRow.Append(TextCell(text, styleIndex: 1));
            sheetData.Append(headerRow);

            // Local predicate: is this a numeric column (based on sheet type and index)
            bool IsNumericCol(int colIndex) =>
                sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase) && colIndex >= 1 && colIndex <= maxAns
                || (sheetName.Equals("Egyválasztós", StringComparison.OrdinalIgnoreCase) ||
                    sheetName.Equals("Többválasztós", StringComparison.OrdinalIgnoreCase)) && colIndex >= 1 && colIndex <= maxAns;

            // Write data rows
            foreach (var (Main, Opts) in blocks)
            {
                var dataRow = new Row();
                for (int c = 0; c < Main.Count; c++)
                {
                    if (IsNumericCol(c) &&
                        double.TryParse(Main[c], NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                        dataRow.Append(NumberCell(num, styleIndex: 4));
                    else
                        dataRow.Append(TextCell(Main[c], styleIndex: 2));
                }
                sheetData.Append(dataRow);

                // Options row (if present)
                if (Opts is { Count: > 1 })
                {
                    var optRow = new Row();
                    for (int c = 0; c < Opts.Count; c++)
                        optRow.Append(TextCell(Opts[c], styleIndex: 3));
                    sheetData.Append(optRow);
                }
            }
        }

        // ---------------- Helpers: styles & cells ----------------

        /// <summary>
        /// Creates a text cell (InlineString) with the given style index.
        /// </summary>
        /// <param name="text">Cell text (empty string if null).</param>
        /// <param name="styleIndex">Cell format style index.</param>
        private static Cell TextCell(string? text, uint styleIndex = 0) =>
            new()
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty)),
                StyleIndex = styleIndex
            };

        /// <summary>
        /// Creates a numeric cell (Number) using InvariantCulture formatting.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <param name="styleIndex">Cell format style index.</param>
        private static Cell NumberCell(double value, uint styleIndex = 0) =>
            new()
            {
                CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture)),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };

       
        /// <summary>
        /// Returns an estimated column width based on text length and role (question / numeric / other).
        /// </summary>
        /// <param name="text">Cell content (supports multiline).</param>
        /// <param name="isQuestionCol">Whether this is the question column (A column).</param>
        /// <param name="isNumericCol">Whether this is a numeric column (Likert/SC/MC answer columns).</param>
        /// <returns>Width in Excel units, clamped to a reasonable min–max.</returns>
        private static double EstimateWidth(string? text, bool isQuestionCol, bool isNumericCol)
        {
            var t = text ?? string.Empty;
            var maxLine = t.Split('\n').Max(s => s.Length);
            var baseLen = Math.Max(1, maxLine);

            var pad = isNumericCol ? 1.0 : 2.0;
            var w = baseLen + pad;

            if (isQuestionCol) return Math.Clamp(w, 14.0, 80.0);
            if (isNumericCol) return Math.Clamp(w, 6.0, 30.0);
            return Math.Clamp(w, 8.0, 60.0);
        }

        /// <summary>
        /// Builds column widths by scanning the header and data rows.
        /// </summary>
        /// <param name="header">Header row.</param>
        /// <param name="blocks">Data blocks (Main + Opts).</param>
        /// <param name="sheetName">Sheet name (influences numeric column detection).</param>
        /// <param name="maxAns">Number of numeric answer columns.</param>
        /// <returns><see cref="Columns"/> collection with per-column widths.</returns>
        private static Columns BuildAutoColumns(
            IReadOnlyList<string> header,
            IReadOnlyList<(List<string> Main, List<string> Opts)> blocks,
            string sheetName,
            int maxAns)
        {
            int colCount = header.Count;
            var maxWidths = new double[colCount];

            bool IsNumericCol(int colIndex) =>
                sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase) && colIndex >= 1 && colIndex <= maxAns
                || (sheetName.Equals("Egyválasztós", StringComparison.OrdinalIgnoreCase) ||
                    sheetName.Equals("Többválasztós", StringComparison.OrdinalIgnoreCase)) && colIndex >= 1 && colIndex <= maxAns;

            // Header widths
            for (int c = 0; c < colCount; c++)
                maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(header[c], c == 0, IsNumericCol(c)));

            // Data row widths
            foreach (var (Main, Opts) in blocks)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (c < Main.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(Main[c], c == 0, IsNumericCol(c)));
                    if (c < Opts.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(Opts[c], c == 0, false));
                }
            }

            // Build Columns (individual widths)
            var cols = new Columns();
            for (uint i = 0; i < colCount; i++)
            {
                cols.Append(new Column { Min = i + 1, Max = i + 1, Width = maxWidths[i], CustomWidth = true });
            }
            return cols;
        }

        /// <summary>
        /// Sets the maximum value (count of answers/options) for the given sheet type.
        /// </summary>
        /// <param name="map">Target dictionary.</param>
        /// <param name="sheet">Sheet type key.</param>
        /// <param name="candidate">New candidate maximum.</param>
        private static void UpdateMax(Dictionary<string, int> map, string sheet, int candidate)
        {
            if (map.TryGetValue(sheet, out var curr)) map[sheet] = Math.Max(curr, candidate);
            else map[sheet] = candidate;
        }

        /// <summary>
        /// Creates an Excel-compatible, unique worksheet name:
        /// removes invalid characters, applies the 31-character limit,
        /// and adds numbering in case of collisions (" (2)", " (3)", ...).
        /// </summary>
        /// <param name="raw">Original sheet name.</param>
        /// <param name="used">Already used names (case-insensitive).</param>
        /// <returns>Safe, unique sheet name.</returns>
        private static string MakeSafeSheetName(string raw, HashSet<string> used)
        {
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var name = new string(raw.Where(ch => !invalid.Contains(ch)).ToArray());
            if (string.IsNullOrWhiteSpace(name)) name = "Sheet";
            if (name.Length > 31) name = name[..31];

            var baseName = name;
            int i = 2;
            while (!used.Add(name))
            {
                var suffix = $" ({i++})";
                var baseLen = Math.Min(31 - suffix.Length, baseName.Length);
                name = baseName[..baseLen] + suffix;
            }
            return name;
        }
    }
}
