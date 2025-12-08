
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model
{
    /// <summary>
    /// Layout configuration for rendering an Excel worksheet.
    /// </summary>
    public sealed class SheetLayoutConfig
    {

        /// <summary>
        /// Maximum number of answer columns across all questions in this sheet
        /// </summary>
        public int MaxAnswerColumns { get; init; }

        /// <summary>
        /// Maximum number of option columns across all questions in this sheet.
        /// </summary>
        public int MaxOptionColumns { get; init; }

        /// <summary>
        /// Total number of columns in the sheet.
        /// </summary>
        public int TotalColumns { get; init; }
    }
}
