
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model
{
    /// <summary>
    /// Represents a single question's data block in an Excel worksheet.
    /// </summary>
    public sealed class QuestionBlockModel
    {
        /// <summary>
        /// The main data row containing the question text and answer values.
        /// </summary>
        public IReadOnlyList<string> MainRow { get; init; } = [];

        /// <summary>
        /// The options row containing predefined answer choices (if applicable).
        /// </summary>
        public IReadOnlyList<string> OptionsRow { get; init; } = [];
    }
}
