using Core.Interfaces;

namespace Core.DomainModels
{

    public sealed class QuestionItem : IAggregateProperty
    {
        // Unique identifier for the question item
        public string QuestionID { get; set; } = default!;
        // Type of the question (e.g., multiple choice, open-ended)
        public QuestionItemType QuestionType { get; set; } = default!;
        // The actual question statement presented to users
        public string QuestionStatement { get; set; } = default!;
        // Collection of possible answer options for the question
        public ICollection<string> AnswerOptions { get; set; } = default!;
        // Collection of dependencies that determine the visibility or relevance of the question
        public ICollection<QuestionDependency> Dependencies { get; set; } = default!;
    }
}
