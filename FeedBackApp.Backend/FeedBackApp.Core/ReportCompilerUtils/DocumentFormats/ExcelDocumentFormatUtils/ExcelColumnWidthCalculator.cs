using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.Model.Enum;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{
    /// <summary>
    /// Provides methods for estimating and calculating optimal Excel column widths based on cell content and column
    /// type.
    /// </summary>
    internal class ExcelColumnWidthCalculator
    {

        /// <summary>
        /// Returns an estimated column width based on text length and role (question / numeric / other).
        /// </summary>
        /// <param name="text">Cell content (supports multiline).</param>
        /// <param name="isQuestionCol">Whether this is the question column (A column).</param>
        /// <param name="isNumericCol">Whether this is a numeric column (Likert/SC/MC answer columns).</param>
        /// <returns>Width in Excel units, clamped to a reasonable min–max.</returns>
        internal static double EstimateColumnWidth(string? text, bool isQuestionCol, bool isNumericCol)
        {
            var trimmedText = text ?? string.Empty;
            var maxLine = trimmedText.Split('\n').Max(s => s.Length);
            var baseLength = Math.Max(1, maxLine);

            var padding = isNumericCol ? 1.0 : 2.0;
            var width = baseLength + padding;

            if (isQuestionCol) return Math.Clamp(width, 14.0, 80.0);
            if (isNumericCol) return Math.Clamp(width, 6.0, 30.0);
            return Math.Clamp(width, 8.0, 60.0);
        }

        /// <summary>
        /// Builds column widths by scanning the header and data rows.
        /// </summary>
        /// <param name="header">Header row.</param>
        /// <param name="blocks">Data blocks (Main + Opts).</param>
        /// <param name="sheetName">Sheet name (influences numeric column detection).</param>
        /// <param name="maxAnswerColumns">Number of numeric answer columns.</param>
        /// <returns><see cref="Columns"/> collection with per-column widths.</returns>
        internal static Columns CalculateColumnWidths(
            IReadOnlyList<string> header,
            IReadOnlyList<(List<string> Main,List<string> Opts)> blocks,
            QuestionType questionType,
            int maxAnswerColumns)
        {
            int colCount = header.Count;
            var maxWidths = new double[colCount];

            bool IsNumericColumn(int columnIndex) =>
                 questionType.HasNumericAnswers() &&  
                 columnIndex >= 1 &&
                 columnIndex <= maxAnswerColumns;

            // Header widths
            for (int c = 0; c < colCount; c++)
                maxWidths[c] = Math.Max(maxWidths[c], EstimateColumnWidth(header[c], c == 0, IsNumericColumn(c)));

            // Data row widths
            foreach (var (Main, Opts) in blocks)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (c < Main.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateColumnWidth(Main[c], c == 0, IsNumericColumn(c)));
                    if (c < Opts.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateColumnWidth(Opts[c], c == 0, false));
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

    }
}
