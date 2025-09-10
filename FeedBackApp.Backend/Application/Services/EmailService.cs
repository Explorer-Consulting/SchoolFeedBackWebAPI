using Application.Services.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly string _appPassword;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailRepository _emailRepository;
    private readonly IQuestionnaireRepository _questionnaireRepository;
    private static short DAILY_EMAIL_LIMIT = 500;
    private static short DNS_PORT = 587;

    public EmailService(ILogger<EmailService> logger, IEmailRepository emailRepository, IQuestionnaireRepository questionnaireRepository)
    {
        _fromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not set.");
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? throw new InvalidOperationException("EMAIL_FROM_NAME is not set.");
        _appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") ?? throw new InvalidOperationException("EMAIL_APP_PASSWORD is not set.");
        _logger = logger;
        _emailRepository = emailRepository;
        _questionnaireRepository = questionnaireRepository;
    }

    public async Task<bool> SendEmailBatchAsync()
    {
        try
        {
            var doc = await _emailRepository.GetEmailsDocumentAsync();
            if (doc == null || !doc.EmailsToSendList.Any())
                return false;

            // clean up expired surveys if any remain
            var expired = doc.EmailsToSendList
                .Where(s => s.EndDate < DateTime.UtcNow && s.Role == FeedBackApp.Core.Model.Enum.Role.Student)
                .ToList();

            foreach (var survey in expired)
            {
                doc.EmailsToSendList.Remove(survey);
                _logger.LogInformation("Removed expired survey {SurveyName} ({SurveyId})", survey.SurveyName, survey.SurveyId);
            }

            var activeSurveys = doc.EmailsToSendList
                .Where(s => s.StartDate <= DateTime.UtcNow)
                .ToList();

            if (!activeSurveys.Any())
                return false;

            var batch = activeSurveys
                .SelectMany(s => s.Emails.Select(e => new
                {
                    SurveyId = s.SurveyId,
                    SurveyName = s.SurveyName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Email = e,
                    Role = s.Role
                }))
                .Take(DAILY_EMAIL_LIMIT)
                .ToList();


            if (!batch.Any())
                return false;

            using var smtp = new SmtpClient("smtp.gmail.com", DNS_PORT)
            {
                Credentials = new NetworkCredential(_fromAddress, _appPassword),
                EnableSsl = true
            };

            foreach (var entry in batch)
            {
                var from = new MailAddress(_fromAddress, _fromName);
                var to = new MailAddress(entry.Email);

                string subject;
                string body;
                List<Attachment>? attachments = null;

                switch (entry.Role)
                {
                    case Role.Student:
                        subject = $"Survey Invitation: {entry.SurveyName}";
                        body = $@"Hello,<br/><br/>
                      Please complete the survey <b>{entry.SurveyName}</b> for teacher feedback.<br/><br/>
                      <a href=""https://witty-beach-0b0c08903.2.azurestaticapps.net"">
                      Click here to start the survey</a>";
                        break;

                    case Role.Teacher:
                        subject = $"Survey Results for {entry.SurveyName}";
                        body = $@"Hello Teacher,<br/><br/>
                      Attached you’ll find the survey results for <b>{entry.SurveyName}</b>.";
                        break;

                    case Role.Admin:
                        subject = $"[Admin Report] Survey {entry.SurveyName}";
                        body = $@"Hello Admin,<br/><br/>
                      Please find attached the administrative report for survey 
                      <b>{entry.SurveyName}</b>.";
                        break;

                    default:
                        _logger.LogWarning("Unhandled role {Role} for email {Email}", entry.Role, entry.Email);
                        continue;
                }

                using var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                if (attachments != null)
                {
                    foreach (var att in attachments)
                        message.Attachments.Add(att);
                }

                await smtp.SendMailAsync(message);
                _logger.LogInformation("Sent email to {Email} for survey {SurveyName} (Role: {Role})",
                                       entry.Email, entry.SurveyName, entry.Role);
            }

            // remove sent emails from the document
            foreach (var e in batch)
            {
                var surveyBatch = doc.EmailsToSendList.FirstOrDefault(s => s.SurveyId == e.SurveyId);
                if (surveyBatch != null)
                    surveyBatch.Emails.Remove(e.Email);

                if (surveyBatch != null && !surveyBatch.Emails.Any())
                {
                    doc.EmailsToSendList.Remove(surveyBatch);
                }
            }

            await _emailRepository.UpdateEmailsDocumentAsync(doc);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending batch of emails");
            return false;
        }
    }

    public async Task CompileReportEmailsAsync(Guid surveyId)
    {
        var metadata = await _questionnaireRepository.GetSurveyMetadataAsync(surveyId);
        if (metadata != null)
        {
            var teachers = metadata.Teachers
             .Where(t => !string.IsNullOrWhiteSpace(t.Email))
             .Select(t => t.Email)
             .ToList();

            if (!teachers.Any())
                return;

            var emailDocument = await _emailRepository.GetEmailsDocumentAsync();

            if (emailDocument == null)
            {
                emailDocument = new EmailsToSend
                {
                    EmailsToSendList = new List<Email>()
                };
            }

            var teacherEmailsToSend = new Email()
            {
                Emails = teachers,
                StartDate = metadata.StartDate,
                EndDate = metadata.EndDate,
                Role = Role.Teacher,
                SurveyId = surveyId.ToString(),
                SurveyName = metadata.Title
            };
            emailDocument.EmailsToSendList.Add(teacherEmailsToSend);

            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var adminEmailsToSend = new Email()
            {
                Emails = adminEmails,
                StartDate = metadata.StartDate,
                EndDate = metadata.EndDate,
                Role = Role.Admin,
                SurveyId = surveyId.ToString(),
                SurveyName = metadata.Title
            };
            emailDocument.EmailsToSendList.Add(adminEmailsToSend);

            await _emailRepository.UpdateEmailsDocumentAsync(emailDocument);
        }
    }
}
