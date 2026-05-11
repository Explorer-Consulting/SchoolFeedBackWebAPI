using FeedBackApp.Core.Email.Models;
using Microsoft.Extensions.Logging;

namespace Application.Email.Templates;

/// <summary>
/// Service for rendering email templates with token replacement.
/// Templates are loaded at application startup and cached in memory for optimal performance.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IReadOnlyDictionary<string, EmailTemplate> _templates;
    private readonly ILogger<EmailTemplateService> _logger;

    /// <summary>
    /// Initializes the template service with pre-loaded templates.
    /// Templates should be loaded at application startup and passed here.
    /// </summary>
    public EmailTemplateService(
        IReadOnlyDictionary<string, EmailTemplate> templates,
        ILogger<EmailTemplateService> logger)
    {
        _templates = templates ?? throw new ArgumentNullException(nameof(templates));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders an email message from a template by replacing tokens with provided values.
    /// </summary>
    public Task<EmailMessage> RenderEmailAsync(
        string templateId,
        string recipientEmail,
        Dictionary<string, string> tokens,
        List<EmailAttachment>? attachments = null)
    {
        if (!_templates.TryGetValue(templateId, out var template))
        {
            throw new KeyNotFoundException($"Email template '{templateId}' not found. Available templates: {string.Join(", ", _templates.Keys)}");
        }

        // Replace tokens in subject and body
        var subject = ReplaceTokens(template.SubjectTemplate, tokens);
        var body = ReplaceTokens(template.BodyTemplate, tokens);

        var message = new EmailMessage
        {
            To = recipientEmail,
            Subject = subject,
            Body = body,
            IsHtml = template.IsHtml,
            Attachments = attachments ?? new List<EmailAttachment>()
        };

        return Task.FromResult(message);
    }

    /// <summary>
    /// Replaces tokens in a template string (e.g., {{SurveyName}} -> actual value).
    /// </summary>
    private static string ReplaceTokens(string template, Dictionary<string, string> tokens)
    {
        var result = template;
        foreach (var (key, value) in tokens)
        {
            result = result.Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }
}

