
namespace Application.DTOs.Email
{
    public class PendingEmailDTO
    {
        public string SurveyId { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
