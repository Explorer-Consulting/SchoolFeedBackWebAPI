namespace Application.Email.Models;

/// <summary>
/// Data transfer object representing a pending email to be sent.
/// </summary>
public class PendingEmailDTO
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
}

