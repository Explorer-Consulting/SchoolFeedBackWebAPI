
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model
{
    /// <summary>
    /// Represents a single question's data block in an Excel worksheet.
    /// </summary>
    public sealed class QuestionBlock
    {
        /// <summary>
        /// The header rows containing the question text and metadata.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> HeaderRows { get; init; } = [];

        /// <summary>
        /// The options rows containing predefined answer choices.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> OptionRows { get; init; } = [];

        /// <summary>
        /// The answers rows containts the answers and their number. 
        /// </summary> 
        public IReadOnlyList<IReadOnlyList<string>> AnswerRows { get; init; } = [];
    }
}
