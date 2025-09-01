
namespace FeedBackApp.Core.Model
{
    public class Questionnaire
    {
        public string Id { get; set; } = string.Empty;

        public string SurveyId { get; set; } = string.Empty;

        public string TeacherEmail { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public bool Status { get; set; } = false;
             
        public IList<QuestionAnswer> QuestionnaireResults { get; set; } = new List<QuestionAnswer>();
    }
}
