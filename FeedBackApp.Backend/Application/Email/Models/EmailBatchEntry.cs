using FeedBackApp.Core.Model.Enum;

namespace Application.Email.Models;

/// <summary>
/// Represents a single email entry in a batch to be sent.
/// </summary>
public class EmailBatchEntry
{
    /// <summary>
    /// Survey identifier.
    /// </summary>
    public string SurveyId { get; set; } = string.Empty;

    /// <summary>
    /// Survey name.
    /// </summary>
    public string SurveyName { get; set; } = string.Empty;

    /// <summary>
    /// Survey start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Survey end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Recipient email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Role of the recipient (Student, Teacher, Admin).
    /// </summary>
    public Role Role { get; set; }
}

