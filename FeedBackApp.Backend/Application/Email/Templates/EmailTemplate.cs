namespace Application.Email.Templates;

/// <summary>
/// Represents an email template with subject and body that can be used to generate email messages.
/// Templates are loaded at application startup and cached in memory.
/// </summary>
public class EmailTemplate
{
    /// <summary>
    /// Template identifier (e.g., "survey-invitation", "report-with-attachments").
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Email subject template with placeholders (e.g., "Kérdőív meghívó: {{SurveyName}}").
    /// </summary>
    public string SubjectTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Email body template with placeholders (e.g., "Kedves diák,<br/>Kérünk töltsd ki a <b>{{SurveyName}}</b>...").
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the body contains HTML content.
    /// </summary>
    public bool IsHtml { get; set; } = true;
}

