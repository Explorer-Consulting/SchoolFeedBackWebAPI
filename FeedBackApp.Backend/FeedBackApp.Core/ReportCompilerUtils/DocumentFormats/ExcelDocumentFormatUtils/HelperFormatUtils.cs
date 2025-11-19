using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{
    internal class HelperFormatUtils
    {
        // ---------------- Helpers: styles & cells ----------------

        /// <summary>
        /// Creates a text cell (InlineString) with the given style index.
        /// </summary>
        /// <param name="text">Cell text (empty string if null).</param>
        /// <param name="styleIndex">Cell format style index.</param>
        internal static Cell TextCell(string? text, uint styleIndex = 0) =>
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
        internal static Cell NumberCell(double value, uint styleIndex = 0) =>
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
        internal static double EstimateWidth(string? text, bool isQuestionCol, bool isNumericCol)
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
        internal static Columns BuildAutoColumns(
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
        internal static void UpdateMax(Dictionary<string, int> map, string sheet, int candidate)
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
        internal static string MakeSafeSheetName(string raw, HashSet<string> used)
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
