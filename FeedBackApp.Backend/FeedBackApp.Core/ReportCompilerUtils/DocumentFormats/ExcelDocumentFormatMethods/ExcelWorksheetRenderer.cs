using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model.Enum;
using System.Globalization;


namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods
{
    internal static class ExcelWorksheetRenderer
    {

        /// <summary>
        /// Render a worksheet and populates it with the specified header and data rows (Main + Opts).
        /// </summary>
        /// <param name="wbPart">The workbook part.</param>
        /// <param name="sheets">The workbook’s sheet collection.</param>
        /// <param name="sheetName">The worksheet name (Excel-compatible, see <see cref="MakeSafeSheetName"/>).</param>
        /// <param name="header">The main table header (first row).</param>
        /// <param name="blocks">The normalized blocks (Main row and optional Opts row).</param>
        /// <param name="explicitSheetId">Optional sheet ID.</param>
        /// <param name="maxAns">The maximum number of answer columns on this sheet.</param>
        /// <param name="maxOptionColumns">The maximum number of option columns on this sheet.</param>
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
        internal static void RenderWorksheet(
            WorkbookPart wbPart,
            Sheets sheets,
            string sheetName,
            QuestionType questionType,
            IReadOnlyList<(List<string> Row, RowType Type)> rows,
            uint? explicitSheetId,
            int maxAnswerColumns,
            int maxOptionColumns)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();

            // Column width estimation based on content
            var cols = ExcelColumnWidthCalculator.CalculateColumnWidths(rows, questionType, maxAnswerColumns);

            // Freeze header (Pane)
            var views = new SheetViews(new SheetView
            {
                WorkbookViewId = 0,
                Pane = new Pane
                {
                    VerticalSplit = 1D,
                    TopLeftCell = "A2",
                    ActivePane = PaneValues.BottomLeft,
                    State = PaneStateValues.Frozen
                }
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

            // Local predicate: is this a numeric column (based on sheet type and index)
            bool IsNumericColumn(int columnIndex) =>
                questionType.HasNumericAnswers() &&
                columnIndex >= 1 &&
                columnIndex <= maxAnswerColumns;

            // Write data rows
            foreach (var (row, rowType) in rows)
            {
                var excelRow = new Row();
                for (int c = 0; c < row.Count; c++)
                {

                    // Determine style based on row type
                    if (rowType == RowType.Header)
                    {
                        excelRow.Append(CreateTextCell(row[c], styleIndex: 1));
                    }
                    else if (rowType == RowType.Option)
                    {
                        // Options always get style 3 (italic, light background)
                        excelRow.Append(CreateTextCell(row[c], styleIndex: 3));
                    }
                    else if (IsNumericColumn(c) &&
                             double.TryParse(row[c], NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                    {
                        // Numeric data in answer/header rows
                        excelRow.Append(CreateNumberCell(num, styleIndex: 4));
                    }
                    else if (rowType == RowType.Answer)
                    {
                        // Text data in answer/header rows
                        excelRow.Append(CreateTextCell(row[c], styleIndex: 2));
                    }
                }
                sheetData.Append(excelRow);
            }
        }

        /// <summary>
        /// Creates a text cell (InlineString) with the given style index.
        /// </summary>
        /// <param name="text">Cell text (empty string if null).</param>
        /// <param name="styleIndex">Cell format style index.</param>
        private static Cell CreateTextCell(string? text, uint styleIndex = 0) =>
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
        internal static Cell CreateNumberCell(double value, uint styleIndex = 0) =>
            new()
            {
                CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture)),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };

    }
}
