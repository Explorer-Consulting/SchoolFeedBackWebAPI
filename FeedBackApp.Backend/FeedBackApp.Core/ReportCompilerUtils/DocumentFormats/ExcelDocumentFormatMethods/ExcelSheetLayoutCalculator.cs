using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model.Enum;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods
{
    /// <summary>
    /// Utility class for calculating Excel worksheet layout configuration.
    /// <para>
    /// Responsible for computing layout-specific information (column counts, header rows)
    /// from domain sheet models.
    /// </para>
    /// </summary>
    internal static class ExcelSheetLayoutCalculator
    {
        /// <summary>
        /// Calculates the layout configuration for a sheet model.
        /// <para>
        /// Determines the maximum number of answer/option columns across all blocks,
        /// generates the appropriate header row based on question type,
        /// and calculates the total column count needed for rendering.
        /// </para>
        /// </summary>
        /// <param name="model">The sheet model containing question blocks.</param>
        /// <returns>Layout configuration with column counts and header row.</returns>

        public static SheetLayoutConfig CalculateLayout(SheetModel model)
        {
            int maxAnswerColumns = 0;
            int maxOptionColumns = 0;

            foreach (var block in model.Blocks)
            {
                // Check header rows 
                foreach (var row in block.HeaderRows)
                    maxAnswerColumns = Math.Max(maxAnswerColumns, row.Count);

                // Check option rows
                foreach (var row in block.OptionRows)
                    maxOptionColumns = Math.Max(maxOptionColumns, row.Count);

                // Check answer rows
                foreach (var row in block.AnswerRows)
                    maxAnswerColumns = Math.Max(maxAnswerColumns, row.Count);
            }

            int totalColumns = Math.Max(maxAnswerColumns, maxOptionColumns);

            return new SheetLayoutConfig
            {
                MaxAnswerColumns = maxAnswerColumns,
                MaxOptionColumns = maxOptionColumns,
                TotalColumns = totalColumns
            };
        }

        /// <summary>
        /// Normalizes question blocks to a consistent column width.
        /// <para>
        /// Pads all rows (main and options) to match the total column count
        /// specified in the layout configuration.
        /// </para>
        /// </summary>
        /// <param name="blocks">The question blocks to normalize.</param>
        /// <param name="layout">The layout configuration with target column counts.</param>
        /// <returns>Normalized blocks as tuples (for compatibility with rendering layer).</returns>
        public static List<(List<string> Row, RowType Type)> NormalizeBlocks(IReadOnlyList<QuestionBlock> blocks, SheetLayoutConfig layout)
        {
            var normalized = new List<(List<string>, RowType)>();
            foreach (var block in blocks)
            {
                // Add all header rows
                foreach (var row in block.HeaderRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);
                    normalized.Add((normalizedRow, RowType.Header));
                }

                // Add all option rows
                foreach (var row in block.OptionRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);
                    normalized.Add((normalizedRow, RowType.Option));
                }

                var emptyRow = new List<string>();
                for (int i = 0; i < layout.TotalColumns; i++)
                    emptyRow.Add(string.Empty);
                normalized.Add((emptyRow, RowType.Empty));

                // Add all answer rows
                bool firstAnswerRow = true;
                foreach (var row in block.AnswerRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);

                    if (firstAnswerRow)
                    {
                        normalized.Add((normalizedRow, RowType.Header));
                        firstAnswerRow = false;
                    }
                    else
                    {
                        normalized.Add((normalizedRow, RowType.Answer));
                    }
                }
            }

            return normalized;
        }
    }
}
