
namespace FeedBackApp.Core.Model
{
    public class QuestionnaireTemplate
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        
        public IList<QuestionTemplate> QuestionTemplates { get; set; } = new List<QuestionTemplate>();

        public QuestionnaireTemplate() { }

        public QuestionnaireTemplate(string id, string title, IList<QuestionTemplate> questionTemplates)
        {
            Id = $"questiontemplates_{id}";
            Title = title;
            int qId = 0;
            foreach (var question in questionTemplates)
            {
                question.Id = $"q{qId++}";
            }
            QuestionTemplates = questionTemplates;
        }

        public bool SelfEnrollmentAllowed { get; set; } = false;
    }
}
