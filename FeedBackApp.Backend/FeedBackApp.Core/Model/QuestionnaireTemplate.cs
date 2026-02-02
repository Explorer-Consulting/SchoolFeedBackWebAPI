
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
        
        // Self opt-in controls
        public bool IsSelfOptInEnabled { get; set; } = false;
        public DateTimeOffset? OptInExpiresAt { get; set; } = null;
        public int? MaxParticipants { get; set; } = null;

        // Instant revocation knob (bump to invalidate all existing links)  
        public int LinkVersion { get; set; } = 1;
        
        // ULID alias for links
        public string TemplateUlid { get; set; } = string.Empty;
        
    }
}
