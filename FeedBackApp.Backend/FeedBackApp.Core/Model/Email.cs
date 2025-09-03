
namespace FeedBackApp.Core.Model
{
    public class Email
    {
        public string SurveyId { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<string> Emails { get; set; } = new List<string>();
    }
}
