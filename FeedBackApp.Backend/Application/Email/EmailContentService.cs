using Application.Email.Templates;
using Application.Services.Interfaces;
using FeedBackApp.Core.Email.Constants;
using FeedBackApp.Core.Email.Models;
using Microsoft.Extensions.Logging;

namespace Application.Email;

/// <summary>
/// Service for creating email messages using templates.
/// This service is responsible for determining the appropriate email template
/// and preparing the data needed for template rendering.
/// </summary>
public class EmailContentService : IEmailContentService
{
    private readonly IEmailTemplateService _templateService;
    private readonly IReportService _reportService;
    private readonly ILogger<EmailContentService> _logger;

    public EmailContentService(
        IEmailTemplateService templateService,
        IReportService reportService,
        ILogger<EmailContentService> logger)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates an email message for a survey invitation (sent to students).
    /// </summary>
    public Task<EmailMessage> CreateSurveyInvitationEmailAsync(
        string recipientEmail,
        string surveyName)
    {
        var tokens = new Dictionary<string, string>
        {
            { "SurveyName", surveyName },
            { "FrontendUrl", EmailConstants.FrontendUrl }
        };

        return _templateService.RenderEmailAsync("survey-invitation", recipientEmail, tokens);
    }

    /// <summary>
    /// Creates an email message with report attachments (sent to teachers).
    /// </summary>
    public async Task<EmailMessage> CreateReportEmailAsync(
        string recipientEmail,
        string surveyName,
        string surveyId)
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

        var tokens = new Dictionary<string, string>
        {
            { "SurveyName", surveyName }
        };

        var message = await _templateService.RenderEmailAsync("report-with-attachments", recipientEmail, tokens, attachments);
        return message;
    }

    /// <summary>
    /// Creates an email message with admin summary reports (sent to administrators).
    /// </summary>
    public async Task<EmailMessage> CreateAdminSummaryEmailAsync(
        string recipientEmail,
        string surveyName,
        string surveyId)
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

        var tokens = new Dictionary<string, string>
        {
            { "SurveyName", surveyName }
        };

        var message = await _templateService.RenderEmailAsync("admin-summary", recipientEmail, tokens, attachments);
        return message;
    }

    /// <summary>
    /// Creates an email message with an OTP code for passwordless authentication.
    /// </summary>
    public Task<EmailMessage> CreateOtpEmailAsync(string recipientEmail, string otpCode)
    {
        var tokens = new Dictionary<string, string>
        {
            { "OtpCode", otpCode }
        };

        return _templateService.RenderEmailAsync("otp-authentication", recipientEmail, tokens);
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

