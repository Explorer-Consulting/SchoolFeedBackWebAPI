
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
    internal class SheetLayoutConfigurationUtils
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
            // Calculate max answer columns
            int maxAnswerColumns = 0;
            int maxOptionColumns = 0;

            foreach (var block in model.Blocks)
            {
                // MainRow: first element is question, rest are answers (+ optional metadata for Likert)
                int answerCount = block.MainRow.Count - 1;  // -1 for question text

                if (model.Type == QuestionType.LikertScaleOneToFive && answerCount > 0)
                    answerCount--; // -1 for ValueMeanings at the end

                maxAnswerColumns = Math.Max(maxAnswerColumns, answerCount);

                // OptionsRow: first element is label ("Opciók"), rest are options
                int optionCount = block.OptionsRow.Count > 0 ? block.OptionsRow.Count - 1 : 0;
                maxOptionColumns = Math.Max(maxOptionColumns, optionCount);
            }

            // Generate header row
            var header = new List<string> { "Kérdés" };

            if (model.Type == QuestionType.LikertScaleOneToFive)
            {
                for (int i = 0; i < maxAnswerColumns; i++)
                    header.Add(string.Empty);

                header.Add("Értékek jelentése");
            }
            else
            {
                for (int i = 0; i < maxAnswerColumns; i++)
                    header.Add(string.Empty);
            }

            // Calculate total columns
            int mainCols = 1 + maxAnswerColumns + (model.Type == QuestionType.LikertScaleOneToFive ? 1 : 0);
            int optionCols = 1 + maxOptionColumns;
            int totalCols = Math.Max(mainCols, optionCols);

            while (header.Count < totalCols)
                header.Add(string.Empty);

            return new SheetLayoutConfig
            {
                MaxAnswerColumns = maxAnswerColumns,
                MaxOptionColumns = maxOptionColumns,
                HeaderRow = header,
                TotalColumns = totalCols
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
        public static List<(List<string> Main, List<string> Opts)> NormalizeBlocks(
        IReadOnlyList<QuestionBlockModel> blocks, SheetLayoutConfig layout)
        {
            var normalized = new List<(List<string> Main, List<string> Opts)>(blocks.Count);

            foreach (var block in blocks)
            {
                var main = new List<string>(block.MainRow);
                var opts = new List<string>(block.OptionsRow);

                // Pad to total columns
                while (main.Count < layout.TotalColumns)
                    main.Add(string.Empty);
                while (opts.Count < layout.TotalColumns)
                    opts.Add(string.Empty);

                normalized.Add((main, opts));
            }

            return normalized;
        }
    }
}
