
using FeedBackApp.Core.Model.Enum;

namespace FeedBackApp.Core.Model
{
    public class QuestionTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public IList<string> AnswerOptions { get; set; } = new List<string>();
        public IList<DropDownItem> DropDownItems { get; set; } = new List<DropDownItem>();
        public QuestionDependency? Dependency { get; set; }
        public required string Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ParentQuestionId { get; set; }
    }
}
