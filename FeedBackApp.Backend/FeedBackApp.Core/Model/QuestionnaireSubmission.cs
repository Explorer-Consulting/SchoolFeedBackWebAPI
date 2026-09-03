namespace FeedBackApp.Core.Model
{
    public class QuestionnaireSubmission
    {
        public bool IsValidate { get; set; }
        public List<QuestionAnswer> QuestionnaireResults { get; set; } = new List<QuestionAnswer>();
    }
}
