using Application.Email.Builders;
using Application.Email.Helpers;
using Application.Services.Interfaces;
using FeedBackApp.Core.Email;
using FeedBackApp.Core.Email.Configuration;
using FeedBackApp.Core.Email.Constants;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;

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
    private readonly IEmailContentFactory _emailContentFactory;
    private readonly IEmailSender _emailSender;

    public EmailService(
        ILogger<EmailService> logger,
        IEmailRepository emailRepository,
        IQuestionnaireRepository questionnaireRepository,
        IEmailContentFactory emailContentFactory,
        IEmailSender emailSender,
        EmailConfiguration emailConfig)
    {
        _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
        _logger = logger;
        _emailRepository = emailRepository;
        _questionnaireRepository = questionnaireRepository;
        _emailContentFactory = emailContentFactory;
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
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
            EmailBatchProcessor.RemoveExpiredStudentSurveys(doc, DateTime.UtcNow);
            
            // Get surveys ready to send
            var activeSurveys = EmailBatchProcessor.GetSurveysReadyToSend(doc, DateTime.UtcNow);
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

            _logger.LogInformation("Created email batch with {Count} entries. Admin emails in batch: {AdminEmails}",
                                   batch.Count,
                                   string.Join(", ", batch.Where(e => e.Role == Role.Admin).Select(e => e.Email)));

            // Send emails using IEmailSender (MailKit implementation) - in parallel with concurrency limit
            // Use semaphore to limit concurrent SMTP connections (Gmail allows ~10 concurrent connections)
            var semaphore = new SemaphoreSlim(10, 10);
            var emailTasks = batch.Select(async entry =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Build email content based on role
                    var emailMessage = await _emailContentFactory.BuildEmailAsync(
                        entry.Email,
                        entry.SurveyName,
                        entry.SurveyId,
                        entry.Role);

                    // Send email using the email sender implementation
                    var success = await _emailSender.SendEmailAsync(emailMessage);
                    
                    if (success)
                    {
                        _logger.LogInformation("Sent email to {Email} for survey {SurveyName} (Role: {Role})",
                                               entry.Email, entry.SurveyName, entry.Role);
                        return (Success: true, Email: entry.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send email to {Email} for survey {SurveyName} (Role: {Role})",
                                          entry.Email, entry.SurveyName, entry.Role);
                        return (Success: false, Email: entry.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception while sending email to {Email} for survey {SurveyName}",
                                     entry.Email, entry.SurveyName);
                    // Continue with next email instead of failing entire batch
                    return (Success: false, Email: entry.Email);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            // Wait for all emails to be sent in parallel
            var results = await Task.WhenAll(emailTasks);
            var successCount = results.Count(r => r.Success);
            
            // Log detailed results for admin emails
            var adminBatchEntries = batch.Where(e => e.Role == Role.Admin).ToList();
            var adminResults = results.Where(r => adminBatchEntries.Any(e => e.Email == r.Email)).ToList();
            if (adminResults.Any())
            {
                _logger.LogInformation("Admin email sending results: {SuccessCount} successful, {FailedCount} failed. " +
                                       "Successful: {SuccessfulEmails}, Failed: {FailedEmails}",
                                       adminResults.Count(r => r.Success),
                                       adminResults.Count(r => !r.Success),
                                       string.Join(", ", adminResults.Where(r => r.Success).Select(r => r.Email)),
                                       string.Join(", ", adminResults.Where(r => !r.Success).Select(r => r.Email)));
            }

            // Remove sent emails from the document
            EmailBatchProcessor.RemoveSentEmails(doc, batch);
            await _emailRepository.UpdateEmailsDocumentAsync(doc);

            _logger.LogInformation(
                "Email batch processing completed. Attempted: {Attempted}, Successful: {Successful}, Failed: {Failed}",
                batch.Count,
                successCount,
                batch.Count - successCount);
            
            return successCount > 0;
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
        _logger.LogInformation("Compiling admin emails for survey {SurveyId}. LeaderEmails config value: '{LeaderEmails}'", 
                               surveyId, 
                               _emailConfig.LeaderEmails ?? "(null)");
        
        var adminEmail = EmailCompilationHelper.CreateAdminEmail(
            metadata, 
            surveyId, 
            _emailConfig.LeaderEmails ?? string.Empty);
        
        if (adminEmail.Emails.Any())
        {
            emailDocument.EmailsToSendList.Add(adminEmail);
            _logger.LogInformation("Added {Count} admin emails for survey {SurveyId}: {Emails}", 
                                   adminEmail.Emails.Count, 
                                   surveyId,
                                   string.Join(", ", adminEmail.Emails));
        }
        else
        {
            _logger.LogWarning("No admin emails to add for survey {SurveyId}. LeaderEmails config: '{LeaderEmails}'", 
                               surveyId, 
                               _emailConfig.LeaderEmails ?? "(null)");
        }

        await _emailRepository.UpdateEmailsDocumentAsync(emailDocument);
        _logger.LogInformation("Successfully compiled report emails for survey {SurveyId}", surveyId);
    }
}
