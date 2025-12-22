using Application.Email;
using Application.Email.Helpers;
using Application.Email.Models;
using Application.Services.Interfaces;
using FeedBackApp.Core.Email;
using FeedBackApp.Core.Email.Configuration;
using FeedBackApp.Core.Email.Constants;
using FeedBackApp.Core.Email.Models;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Services;

/// <summary>
/// Service for managing email sending operations including batch processing and email compilation.
/// </summary>
public class EmailService(
    ILogger<EmailService> logger,
    IEmailRepository emailRepository,
    IQuestionnaireRepository questionnaireRepository,
    IEmailContentService emailContentService,
    IEmailSender emailSender,
    EmailConfiguration emailConfig) : IEmailService
{
    private readonly ILogger<EmailService> _logger = logger;
    private readonly IEmailRepository _emailRepository = ValidateAndAssign(emailRepository);
    private readonly IQuestionnaireRepository _questionnaireRepository = ValidateAndAssign(questionnaireRepository);
    private readonly IEmailContentService _emailContentService = ValidateAndAssign(emailContentService);
    private readonly IEmailSender _emailSender = ValidateAndAssign(emailSender);
    private readonly EmailConfiguration _emailConfig = ValidateAndAssign(emailConfig);

    private static T ValidateAndAssign<T>(T value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        return value;
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

            EmailBatchProcessor.RemoveExpiredSurveys(doc, DateTime.UtcNow);
            
            var activeSurveys = EmailBatchProcessor.GetSurveysReadyToSend(doc, DateTime.UtcNow);
            if (!activeSurveys.Any())
            {
                _logger.LogDebug("No active surveys to send");
                return false;
            }

            var batch = EmailBatchProcessor.CreateBatch(activeSurveys, EmailConstants.DailyEmailLimit);
            if (!batch.Any())
            {
                _logger.LogDebug("Batch is empty after applying daily limit");
                return false;
            }

            _logger.LogInformation("Created email batch with {Count} entries. Admin emails in batch: {AdminEmails}",
                                   batch.Count,
                                   string.Join(", ", batch.Where(e => e.Role == Role.Admin).Select(e => e.Email)));

            // Parallelism is needed because SMTP operations are I/O-bound and can be performed concurrently.
            // This significantly improves throughput when sending multiple emails.
            // Gmail allows ~10 concurrent SMTP connections, so we limit concurrency to avoid rate limiting.
            var results = new ConcurrentBag<(bool Success, string Email)>();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };

            await Parallel.ForEachAsync(batch, parallelOptions, async (entry, ct) =>
            {
                try
                {
                    var emailMessage = await CreateEmailMessageAsync(entry);
                    var success = await _emailSender.SendEmailAsync(emailMessage);
                    
                    if (success)
                    {
                        _logger.LogInformation("Sent email to {Email} for survey {SurveyName} (Role: {Role})",
                                               entry.Email, entry.SurveyName, entry.Role);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send email to {Email} for survey {SurveyName} (Role: {Role})",
                                          entry.Email, entry.SurveyName, entry.Role);
                    }
                    
                    results.Add((success, entry.Email));
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Invalid operation while sending email to {Email} for survey {SurveyName}",
                                     entry.Email, entry.SurveyName);
                    results.Add((false, entry.Email));
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError(ex, "Timeout while sending email to {Email} for survey {SurveyName}",
                                     entry.Email, entry.SurveyName);
                    results.Add((false, entry.Email));
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    _logger.LogError(ex, "Network error while sending email to {Email} for survey {SurveyName}",
                                     entry.Email, entry.SurveyName);
                    results.Add((false, entry.Email));
                }
            });

            var successCount = results.Count(r => r.Success);
            LogAdminEmailResults(batch, results);
            
            EmailBatchProcessor.RemoveSentEmails(doc, batch);
            await _emailRepository.UpdateEmailsDocumentAsync(doc);

            _logger.LogInformation(
                "Email batch processing completed. Attempted: {Attempted}, Successful: {Successful}, Failed: {Failed}",
                batch.Count,
                successCount,
                batch.Count - successCount);
            
            return successCount > 0;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation error sending batch of emails");
            return false;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout error sending batch of emails");
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

        var emailDocument = await _emailRepository.GetEmailsDocumentAsync();
        emailDocument = EmailCompilationHelper.EnsureEmailDocument(emailDocument);

        AddTeacherEmails(emailDocument, metadata, surveyId);
        AddAdminEmails(emailDocument, metadata, surveyId);

        await _emailRepository.UpdateEmailsDocumentAsync(emailDocument);
        _logger.LogInformation("Successfully compiled report emails for survey {SurveyId}", surveyId);
    }

    private void AddTeacherEmails(EmailsToSend emailDocument, SurveyMetadata metadata, Guid surveyId)
    {
        var teacherEmail = EmailCompilationHelper.CreateTeacherEmail(metadata, surveyId);
        if (teacherEmail.Emails.Any())
        {
            emailDocument.EmailsToSendList.Add(teacherEmail);
            _logger.LogInformation("Added {Count} teacher emails for survey {SurveyId}", 
                                   teacherEmail.Emails.Count, surveyId);
        }
        else
        {
            _logger.LogInformation("No teachers with email addresses found for survey {SurveyId}", surveyId);
        }
    }

    private void AddAdminEmails(EmailsToSend emailDocument, SurveyMetadata metadata, Guid surveyId)
    {
        var leaderEmails = _emailConfig.LeaderEmails ?? string.Empty;
        var adminEmail = EmailCompilationHelper.CreateAdminEmail(metadata, surveyId, leaderEmails);
        
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
                               surveyId, leaderEmails);
        }
    }

    private void LogAdminEmailResults(List<EmailBatchEntry> batch, ConcurrentBag<(bool Success, string Email)> results)
    {
        var adminBatchEntries = batch.Where(e => e.Role == Role.Admin).ToList();
        if (!adminBatchEntries.Any())
            return;

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
    }

    private Task<EmailMessage> CreateEmailMessageAsync(EmailBatchEntry entry)
    {
        return entry.Role switch
        {
            Role.Student => _emailContentService.CreateSurveyInvitationEmailAsync(
                entry.Email, entry.SurveyName),
            Role.Teacher => _emailContentService.CreateReportEmailAsync(
                entry.Email, entry.SurveyName, entry.SurveyId),
            Role.Admin => _emailContentService.CreateAdminSummaryEmailAsync(
                entry.Email, entry.SurveyName, entry.SurveyId),
            _ => throw new ArgumentException($"Unsupported role for email creation: {entry.Role}", nameof(entry))
        };
    }
}
