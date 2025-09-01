
namespace FeedBackApp.Core.Model
{
    public class QuestionnaireTemplate
    {
        public string Id { get; set; } = string.Empty;
        public IList<QuestionTemplate> QuestionTemplates { get; set; } = new List<QuestionTemplate>();

        public QuestionnaireTemplate() { }

        public QuestionnaireTemplate(string id, IList<QuestionTemplate> questionTemplates)
        {
            Id = $"questiontemplates_{id}";
            int qId = 0;
            foreach (var question in questionTemplates)
            {
                question.Id = $"q{qId++}";
            }
            QuestionTemplates = questionTemplates;
        }
    }
}
