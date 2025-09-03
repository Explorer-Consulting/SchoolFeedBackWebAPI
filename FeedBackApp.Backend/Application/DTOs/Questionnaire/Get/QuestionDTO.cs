using FeedBackApp.Core.Model.Enum;

namespace Application.DTOs.Questionnaire
{
    public class QuestionDTO
    {
        public string QuestionID { get; set; } = string.Empty;

        public string Question { get; set; } = string.Empty;

        public QuestionType Type { get; set; }

        public IList<string> AnswerOptions { get; set; } = new List<string>();

        public string Answer { get; set; } = string.Empty;
    }
}
