using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Email.Templates;

/// <summary>
/// Loads email templates from configuration at application startup.
/// Templates are loaded once and cached in memory for optimal performance.
/// </summary>
public static class EmailTemplateLoader
{
    /// <summary>
    /// Loads email templates from configuration.
    /// Templates can be defined in appsettings.json or environment variables.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger for tracking template loading.</param>
    /// <returns>Dictionary of template ID to EmailTemplate, loaded and ready to use.</returns>
    public static IReadOnlyDictionary<string, EmailTemplate> LoadTemplates(
        IConfiguration configuration,
        ILogger logger)
    {
        var templates = new Dictionary<string, EmailTemplate>();

        // Load templates from configuration section "Email:Templates"
        var templatesSection = configuration.GetSection("Email:Templates");
        
        if (!templatesSection.Exists())
        {
            logger.LogWarning("Email:Templates configuration section not found. Using default templates.");
            return LoadDefaultTemplates();
        }

        foreach (var templateSection in templatesSection.GetChildren())
        {
            var templateId = templateSection.Key;
            var template = new EmailTemplate
            {
                TemplateId = templateId,
                SubjectTemplate = templateSection["Subject"] ?? string.Empty,
                BodyTemplate = templateSection["Body"] ?? string.Empty,
                IsHtml = templateSection.GetValue<bool>("IsHtml", true)
            };

            if (string.IsNullOrWhiteSpace(template.SubjectTemplate) || string.IsNullOrWhiteSpace(template.BodyTemplate))
            {
                logger.LogWarning("Template '{TemplateId}' is missing Subject or Body. Skipping.", templateId);
                continue;
            }

            templates[templateId] = template;
            logger.LogInformation("Loaded email template: {TemplateId}", templateId);
        }

        if (templates.Count == 0)
        {
            logger.LogWarning("No email templates loaded from configuration. Using default templates.");
            return LoadDefaultTemplates();
        }

        return templates;
    }

    /// <summary>
    /// Loads default templates as fallback when configuration is not available.
    /// These are hardcoded defaults but should be replaced with configuration-based templates.
    /// </summary>
    private static IReadOnlyDictionary<string, EmailTemplate> LoadDefaultTemplates()
    {
        return new Dictionary<string, EmailTemplate>
        {
            {
                "survey-invitation",
                new EmailTemplate
                {
                    TemplateId = "survey-invitation",
                    SubjectTemplate = "Kérdőív meghívó: {{SurveyName}}",
                    BodyTemplate = @"Kedves diák,<br/><br/>
Kérünk töltsd ki a <b>{{SurveyName}}</b> nevű kérdőívet, és adj visszajelzést a tanáraidnak.<br/><br/>
<a href=""{{FrontendUrl}}"">Kattints ide, hogy elkezdd a kérdőívek kitöltését</a>",
                    IsHtml = true
                }
            },
            {
                "report-with-attachments",
                new EmailTemplate
                {
                    TemplateId = "report-with-attachments",
                    SubjectTemplate = "Kérdőív eredmények: {{SurveyName}}",
                    BodyTemplate = @"Kedves tanár,<br/><br/>
Csatolva megtalálja a kérdőívek összesített eredményét <b>{{SurveyName}}</b>.",
                    IsHtml = true
                }
            },
            {
                "admin-summary",
                new EmailTemplate
                {
                    TemplateId = "admin-summary",
                    SubjectTemplate = "Igazgatói összesítés: {{SurveyName}}",
                    BodyTemplate = @"Kedves intézmény vezető,<br/><br/>
Alább csatolotuk a <b>{{SurveyName}}</b> kérdőív összesített eredményeit.",
                    IsHtml = true
                }
            }
        };
    }
}

