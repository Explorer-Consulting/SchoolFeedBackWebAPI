namespace StudentFeedbackSurveyApplication.Domain.AggregateComponents
{
    /// <summary>
    /// Represents a component of a question template, including its order, statement, answer options, type, and
    /// dependencies.
    /// </summary>
    public sealed class QuestionTemplateComponent
    {
        /// <summary>
        /// Gets or sets the unique number assigned to the order.
        /// </summary>
        public required int OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the SQL statement to be executed.
        /// </summary>
        public required string Statement { get; set; }
        /// <summary>
        /// Gets or sets the list of possible answer options.
        /// </summary>
        public required IReadOnlyList<string> AnswerOptions { get; set; } = [];
        /// <summary>
        /// Gets or sets the type of question template to use.
        /// </summary>
        public required QuestionTemplateType Type { get; set; }
        /// <summary>
        /// Gets or sets the collection of dependencies required by this question template.
        /// </summary>
        /// <remarks>Each dependency specifies a prerequisite or related item that must be satisfied or
        /// referenced for the question template to function correctly. Modifying this collection affects which
        /// dependencies are recognized by the template.</remarks>
        public required IReadOnlyList<QuestionTemplateDependency> Dependencies { get; set; } = [];
    }
}
