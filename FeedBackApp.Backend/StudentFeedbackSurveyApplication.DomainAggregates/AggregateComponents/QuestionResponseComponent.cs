namespace StudentFeedbackSurveyApplication.Domain.AggregateComponents
{
    /// <summary>
    /// Represents a response to a question, including its order and the provided answers.
    /// </summary>
    public sealed class QuestionResponseComponent
    {
        /// <summary>
        /// Gets or sets the position of the question within its containing sequence.
        /// </summary>
        public required int QuestionOrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the collection of answers associated with the question.
        /// </summary>
        public required IReadOnlyList<string> QuestionAnswer { get; set; }

    }
}
