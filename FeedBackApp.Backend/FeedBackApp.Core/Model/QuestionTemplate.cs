
using FeedBackApp.Core.Model.Enum;

namespace FeedBackApp.Core.Model
{
    public class QuestionTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public IList<string> AnswerOptions { get; set; } = new List<string>();
        public QuestionDependency? Dependency { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
