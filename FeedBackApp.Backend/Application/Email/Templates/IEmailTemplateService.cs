using FeedBackApp.Core.Email.Models;

namespace Application.Email.Templates;

/// <summary>
/// Service for rendering email templates with token replacement.
/// Templates are loaded at startup and cached in memory for performance.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders an email message from a template by replacing tokens with provided values.
    /// </summary>
    /// <param name="templateId">The template identifier (e.g., "survey-invitation").</param>
    /// <param name="recipientEmail">The recipient's email address.</param>
    /// <param name="tokens">Dictionary of token names and their replacement values (e.g., {{SurveyName}} -> "Math Survey").</param>
    /// <param name="attachments">Optional list of attachments to include.</param>
    /// <returns>Rendered EmailMessage ready to be sent.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when templateId is not found.</exception>
    Task<EmailMessage> RenderEmailAsync(
        string templateId,
        string recipientEmail,
        Dictionary<string, string> tokens,
        List<EmailAttachment>? attachments = null);
}

