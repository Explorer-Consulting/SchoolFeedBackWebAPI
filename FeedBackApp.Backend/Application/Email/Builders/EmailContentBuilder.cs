using Application.Services.Interfaces;
using FeedBackApp.Core.Email.Constants;
using FeedBackApp.Core.Email.Models;
using FeedBackApp.Core.Model.Enum;
using Microsoft.Extensions.Logging;

namespace Application.Email.Builders;

/// <summary>
/// Builds email content for different recipient roles (Student, Teacher, Admin).
/// </summary>
/// 
/*
The name of the class is *Builder. The "Builder" suggests something more like a design pattern implementation, but this class has no business with that pattern. Try a factory or something else, but factory can be also an overkill, if it is overcomplicated.
 
 */
public class EmailContentBuilder : IEmailContentBuilder
{
    private readonly IReportService _reportService;
    private readonly ILogger<EmailContentBuilder> _logger;

    public EmailContentBuilder(
        IReportService reportService,
        ILogger<EmailContentBuilder> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    public async Task<EmailMessage> BuildEmailAsync(
        string recipientEmail,
        string surveyName,
        string surveyId,
        Role role,
        List<EmailAttachment>? attachments = null)
    {
        return role switch
        {
            Role.Student => BuildStudentEmail(recipientEmail, surveyName),
            Role.Teacher => await BuildTeacherEmailAsync(recipientEmail, surveyName, surveyId),
            Role.Admin => await BuildAdminEmailAsync(recipientEmail, surveyName, surveyId),
            _ => throw new ArgumentException($"Unsupported role: {role}", nameof(role))
        };
    }

    private EmailMessage BuildStudentEmail(string recipientEmail, string surveyName)
    {
        return new EmailMessage
        {
            To = recipientEmail,
            Subject = $"Kérdőív meghívó: {surveyName}",
            Body = $@"Kedves diák,<br/><br/>
                      Kérünk töltsd ki a <b>{surveyName}</b> nevű kérdőívet, és adj visszajelzést a tanáraidnak.<br/><br/>
                      <a href=""{EmailConstants.FrontendUrl}"">
                      Kattints ide, hogy elkezdd a kérdőívek kitöltését</a>",
            IsHtml = true,
            Attachments = new List<EmailAttachment>()
        };
    }

    private async Task<EmailMessage> BuildTeacherEmailAsync(string recipientEmail, string surveyName, string surveyId)
    {
        var teacherReports = await _reportService.DownloadTeacherFilesByIdPrefixAsync(recipientEmail, surveyId);
        _logger.LogInformation("Found {Count} teacher reports for {Email} / {SurveyId}",
                               teacherReports.Count, recipientEmail, surveyId);

        var attachments = teacherReports
            .Select(r => new EmailAttachment
            {
                Data = r.Data,
                FileName = r.FileName,
                ContentType = GetContentType(r.FileName)
            })
            .ToList();

        return new EmailMessage
        {
            To = recipientEmail,
            Subject = $"Kérdőív eredmények: {surveyName}",
            Body = $@"Kedves tanár,<br/><br/>
              Csatolva megtalálja a kérdőívek összesített eredményét.<b>{surveyName}</b>.",
            IsHtml = true,
            Attachments = attachments
        };
    }

    private async Task<EmailMessage> BuildAdminEmailAsync(string recipientEmail, string surveyName, string surveyId)
    {
        var adminReports = await _reportService.DownloadAdminFilesByIdPrefixAsync(surveyId);
        _logger.LogInformation("Found {Count} admin reports for {SurveyId}",
                               adminReports.Count, surveyId);

        var attachments = adminReports
            .Select(r => new EmailAttachment
            {
                Data = r.Data,
                FileName = r.FileName,
                ContentType = GetContentType(r.FileName)
            })
            .ToList();

        return new EmailMessage
        {
            To = recipientEmail,
            Subject = $"Igazgatói összesítés: {surveyName}",
            Body = $@"Kedves intézmény vezető,<br/><br/>
              Alább csatolotuk a
              <b>{surveyName}</b> kérdőív összesített eredményeit.",
            IsHtml = true,
            Attachments = attachments
        };
    }

    private static string GetContentType(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            string f when f.EndsWith(".pdf") => "application/pdf",
            string f when f.EndsWith(".xlsx") => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            string f when f.EndsWith(".xls") => "application/vnd.ms-excel",
            _ => "application/octet-stream"
        };
    }
}

