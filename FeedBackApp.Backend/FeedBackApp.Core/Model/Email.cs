using FeedBackApp.Core.Model.Enum;

namespace FeedBackApp.Core.Model;

/// <summary>
/// Represents a collection of email addresses to be sent for a specific survey and role.
/// This model is used to queue email notifications for students, teachers, or administrators.
/// </summary>
    public class Email
    {
    /// <summary>
    /// Gets or sets the unique identifier of the survey.
    /// </summary>
        public string SurveyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the survey.
    /// </summary>
        public string SurveyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date when emails should begin being sent.
    /// Emails will only be sent if the current date is greater than or equal to this date.
    /// </summary>
        public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for email sending.
    /// For student emails, expired surveys (EndDate < current date) will be automatically removed.
    /// </summary>
        public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the list of recipient email addresses for this survey and role.
    /// </summary>
        public IList<string> Emails { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the role of the recipients (Student, Teacher, or Admin).
    /// This determines the email template and content that will be used.
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Checks if the email entry is active and ready to be sent.
    /// </summary>
    /// <param name="currentTime">The current time to compare against (typically UTC).</param>
    /// <returns>True if the start date has been reached and there are email addresses to send to.</returns>
    public bool IsActive(DateTime currentTime)
    {
        return StartDate <= currentTime && Emails.Count > 0;
    }

    /// <summary>
    /// Checks if the email entry has expired (for student emails).
    /// </summary>
    /// <param name="currentTime">The current time to compare against (typically UTC).</param>
    /// <returns>True if the end date has passed and the role is Student.</returns>
    public bool IsExpired(DateTime currentTime)
    {
        return Role == Enum.Role.Student && EndDate < currentTime;
    }
}
