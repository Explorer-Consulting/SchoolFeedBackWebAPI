
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model.Enum
{
    /// <summary>
    /// Represents the type of a row in an Excel worksheet.
    /// Used to determine styling and formatting for different row types.
    /// </summary>
    public enum RowType
    {
        /// <summary>
        /// Header row containing question text and metadata.
        /// </summary>
        Header,

        /// <summary>
        /// Option row containing available answer choices (e.g., Likert scale meanings, multiple choice options).
        /// Displayed with italic font and light background
        /// </summary>
        Option,

        /// <summary>
        /// Answer row containing actual respondent answers.
        /// </summary>
        Answer,

        Empty


    }
}
