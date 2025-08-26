namespace FeedBackApp.Core.Model
{
    public class SurveyMetadata
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IList<StudentSet> StudentSets { get; set; } = new List<StudentSet>();

        public IList<QuestionTemplate> QuestionTemplates { get; set; } = new List<QuestionTemplate>();

        public IList<MetaTeacher> Teachers { get; set; } = new List<MetaTeacher>();

        public IList<QuestionnaireCreationParam> CreationParams { get; set; } = new List<QuestionnaireCreationParam>();
    }
}
