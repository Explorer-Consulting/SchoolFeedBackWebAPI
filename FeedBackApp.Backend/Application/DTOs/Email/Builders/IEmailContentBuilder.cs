using Application.Email.Models;
using FeedBackApp.Core.Model.Enum;

namespace Application.Email.Builders;

/// <summary>
/// Interface for building email content based on recipient role and survey information.
/// </summary>
public interface IEmailContentBuilder
{
    /// <summary>
    /// Builds an email message for the specified role and survey information.
    /// </summary>
    /// <param name="recipientEmail">Email address of the recipient.</param>
    /// <param name="surveyName">Name of the survey.</param>
    /// <param name="surveyId">Identifier of the survey.</param>
    /// <param name="role">Role of the recipient.</param>
    /// <param name="attachments">Optional list of attachments to include.</param>
    /// <returns>Configured EmailMessage instance.</returns>
    Task<EmailMessage> BuildEmailAsync(
        string recipientEmail,
        string surveyName,
        string surveyId,
        Role role,
        List<EmailAttachment>? attachments = null);
}

