using Application.Email.Builders;
using Application.Email.Configuration;
using Application.Email.Constants;
using Application.Email.Helpers;
using Application.Services.Interfaces;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Application.Services;

/// <summary>
/// Service for managing email sending operations including batch processing and email compilation.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailRepository _emailRepository;
    private readonly IQuestionnaireRepository _questionnaireRepository;
    private readonly IEmailContentBuilder _emailContentBuilder;

    public EmailService(
        ILogger<EmailService> logger,
        IEmailRepository emailRepository,
        IQuestionnaireRepository questionnaireRepository,
        IEmailContentBuilder emailContentBuilder)
    {
        _emailConfig = EmailConfiguration.FromEnvironment();
        _logger = logger;
        _emailRepository = emailRepository;
        _questionnaireRepository = questionnaireRepository;
        _emailContentBuilder = emailContentBuilder;
    }

    /// <summary>
    /// Sends a batch of pending emails, respecting daily limits and cleaning up expired surveys.
    /// </summary>
    public async Task<bool> SendEmailBatchAsync()
    {
        try
        {
            var doc = await _emailRepository.GetEmailsDocumentAsync();
            if (doc == null || !doc.EmailsToSendList.Any())
            {
                _logger.LogDebug("No emails to send");
                return false;
            }

            // Clean up expired student surveys
            EmailBatchProcessor.RemoveExpiredSurveys(doc, DateTime.UtcNow);
            
            // Get active surveys ready to send
            var activeSurveys = EmailBatchProcessor.GetActiveSurveys(doc, DateTime.UtcNow);
            if (!activeSurveys.Any())
            {
                _logger.LogDebug("No active surveys to send");
                return false;
            }

            // Create batch respecting daily limit
            var batch = EmailBatchProcessor.CreateBatch(activeSurveys, EmailConstants.DailyEmailLimit);
            if (!batch.Any())
            {
                _logger.LogDebug("Batch is empty after applying daily limit");
                return false;
            }

            // Send emails using SMTP (temporary until we implement IEmailSender)
            using var smtp = new SmtpClient(_emailConfig.SmtpHost, _emailConfig.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailConfig.FromAddress, _emailConfig.AppPassword),
                EnableSsl = true
            };

            foreach (var entry in batch)
            {
                try
                {
                    // Build email content based on role
                    var emailMessage = await _emailContentBuilder.BuildEmailAsync(
                        entry.Email,
                        entry.SurveyName,
                        entry.SurveyId,
                        entry.Role);

                    // Convert to MailMessage and send (temporary adapter)
                    using var mailMessage = MailMessageConverter.ToMailMessage(
                        emailMessage,
                        _emailConfig.FromAddress,
                        _emailConfig.FromName);

                    await smtp.SendMailAsync(mailMessage);
                    _logger.LogInformation("Sent email to {Email} for survey {SurveyName} (Role: {Role})",
                                           entry.Email, entry.SurveyName, entry.Role);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email} for survey {SurveyName}",
                                     entry.Email, entry.SurveyName);
                    // Continue with next email instead of failing entire batch
                }
            }

            // Remove sent emails from the document
            EmailBatchProcessor.RemoveSentEmails(doc, batch);
            await _emailRepository.UpdateEmailsDocumentAsync(doc);

            _logger.LogInformation("Successfully processed email batch. Sent {Count} emails", batch.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch of emails");
            return false;
        }
    }

    /// <summary>
    /// Compiles and queues email notifications for teachers and admins based on survey metadata.
    /// </summary>
    public async Task CompileReportEmailsAsync(Guid surveyId)
    {
        var metadata = await _questionnaireRepository.GetSurveyMetadataAsync(surveyId);
        if (metadata == null)
        {
            _logger.LogWarning("Survey metadata not found for survey {SurveyId}", surveyId);
            return;
        }

        // Check if there are any teachers with email addresses
        var teachers = metadata.Teachers
            .Where(t => !string.IsNullOrWhiteSpace(t.Email))
            .Select(t => t.Email)
            .ToList();

        if (!teachers.Any())
        {
            _logger.LogInformation("No teachers with email addresses found for survey {SurveyId}", surveyId);
            return;
        }

        // Get or create email document
        var emailDocument = await _emailRepository.GetEmailsDocumentAsync();
        emailDocument = EmailCompilationHelper.EnsureEmailDocument(emailDocument);

        // Add teacher emails
        var teacherEmail = EmailCompilationHelper.CreateTeacherEmail(metadata, surveyId);
        emailDocument.EmailsToSendList.Add(teacherEmail);
        _logger.LogInformation("Added {Count} teacher emails for survey {SurveyId}", 
                               teacherEmail.Emails.Count, surveyId);

        // Add admin/leader emails
        var adminEmail = EmailCompilationHelper.CreateAdminEmail(
            metadata, 
            surveyId, 
            _emailConfig.LeaderEmails);
        
        if (adminEmail.Emails.Any())
        {
            emailDocument.EmailsToSendList.Add(adminEmail);
            _logger.LogInformation("Added {Count} admin emails for survey {SurveyId}", 
                                   adminEmail.Emails.Count, surveyId);
        }

        await _emailRepository.UpdateEmailsDocumentAsync(emailDocument);
        _logger.LogInformation("Successfully compiled report emails for survey {SurveyId}", surveyId);
    }
}
