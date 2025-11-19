using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using System.Globalization;
using static FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils.HelperFormatUtils;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentUtils
{
    internal class ExcelCreateSheet
    {

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
        /// 
        internal static void CreateSheet(
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
            var cols = HelperFormatUtils.BuildAutoColumns(header, blocks, sheetName, maxAns);

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
    }
}
