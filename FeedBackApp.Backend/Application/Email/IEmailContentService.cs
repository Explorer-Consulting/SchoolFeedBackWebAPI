using FeedBackApp.Core.Email.Models;

namespace Application.Email;

/// <summary>
/// Service for creating email messages using templates.
/// This interface provides methods for creating different types of emails
/// without exposing role-specific logic to the email sender.
/// </summary>
public interface IEmailContentService
{
    /// <summary>
    /// Creates an email message for a survey invitation (sent to students).
    /// </summary>
    Task<EmailMessage> CreateSurveyInvitationEmailAsync(string recipientEmail, string surveyName);

    /// <summary>
    /// Creates an email message with report attachments (sent to teachers).
    /// </summary>
    Task<EmailMessage> CreateReportEmailAsync(string recipientEmail, string surveyName, string surveyId);

    /// <summary>
    /// Creates an email message with admin summary reports (sent to administrators).
    /// </summary>
    Task<EmailMessage> CreateAdminSummaryEmailAsync(string recipientEmail, string surveyName, string surveyId);
}

