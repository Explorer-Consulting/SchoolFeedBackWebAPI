
using FeedBackApp.Core.Model.Enum;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model
{
    /// <summary>
    /// Domain model representing a single Excel worksheet's data.
    ///</summary>
    public sealed class SheetModel
    {

        /// <summary>
        /// The question type for this sheet.
        /// </summary>
        public QuestionType Type { get; init; }

        /// <summary>
        /// Display name for the sheet, used for UI purposes and Excel sheet naming.
        /// </summary>
        public string DisplayName { get; init; } = "";
        /// <summary>
        /// The question data blocks for this sheet.
        /// </summary>
        public IReadOnlyList<QuestionBlock> Blocks { get; init; } = [];
    }
}
