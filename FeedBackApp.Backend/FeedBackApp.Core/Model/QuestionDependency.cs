
namespace FeedBackApp.Core.Model
{
    public class QuestionDependency
    {
        public string Id { get; set; } = string.Empty;
        public List<int> AnswerConditions { get; set; } = new List<int>();
    }
}
