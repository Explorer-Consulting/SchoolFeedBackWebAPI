
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
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

            // Generate header row
            var header = new List<string>();
            for (int i = 0; i < totalColumns; i++)
                header.Add(string.Empty);

            return new SheetLayoutConfig
            {
                MaxAnswerColumns = maxAnswerColumns,
                MaxOptionColumns = maxOptionColumns,
                HeaderRow = header,
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
        public static List<List<string>> NormalizeBlocks(IReadOnlyList<QuestionBlock> blocks, SheetLayoutConfig layout)
        {
            var normalized = new List<List<string>>();
            foreach (var block in blocks)
            {
                // Add all header rows
                foreach (var row in block.HeaderRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);
                    normalized.Add(normalizedRow);
                }

                // Add all option rows
                foreach (var row in block.OptionRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);
                    normalized.Add(normalizedRow);
                }

                // Add all answer rows
                foreach (var row in block.AnswerRows)
                {
                    var normalizedRow = new List<string>(row);
                    while (normalizedRow.Count < layout.TotalColumns)
                        normalizedRow.Add(string.Empty);
                    normalized.Add(normalizedRow);
                }
            }

            return normalized;
        }
    }
}
