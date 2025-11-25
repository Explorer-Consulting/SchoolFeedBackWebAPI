using FeedBackApp.Core.Model.Enum;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{
    /// <summary>
    /// Extension methods for QuestionType enum.
    /// </summary>
    internal static class QuestionTypeExtensions
    {
        /// <summary>
        /// Gets the display name for a given question type.
        /// </summary>
        public static string GetDisplayName(this QuestionType type) => type switch
        {
            QuestionType.LikertScaleOneToFive => "Likert-skála",
            QuestionType.MultinomialSingleChoice => "Egyválasztós",
            QuestionType.MultiNomialSingleChoiceOther => "Egyválasztós + Nyílt végű kérdés",
            QuestionType.MultipleChoice => "Többválasztós",
            QuestionType.OpenEnded => "Nyílt végű",
            _ => "Ismeretlen"
        };

        /// <summary>
        /// Determines whether a question type has numeric answer columns.
        /// </summary>
        public static bool HasNumericAnswers(this QuestionType type) => type switch
        {
            QuestionType.LikertScaleOneToFive => true,
            QuestionType.MultinomialSingleChoice => true,
            QuestionType.MultiNomialSingleChoiceOther => true,
            QuestionType.MultipleChoice => true,
            QuestionType.OpenEnded => false,
            _ => false
        };
    }
}
