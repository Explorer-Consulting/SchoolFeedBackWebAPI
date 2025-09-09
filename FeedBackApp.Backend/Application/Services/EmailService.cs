using Application.Services.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
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
    private static short DAILY_EMAIL_LIMIT = 500;
    private static short DNS_PORT = 587;

    public EmailService(ILogger<EmailService> logger, IEmailRepository emailRepository)
    {
        _fromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not set.");
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? throw new InvalidOperationException("EMAIL_FROM_NAME is not set.");
        _appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") ?? throw new InvalidOperationException("EMAIL_APP_PASSWORD is not set.");
        _logger = logger;
        _emailRepository = emailRepository;
    }

    public async Task<bool> SendEmailBatchAsync()
    {
        try
        {
            var doc = await _emailRepository.GetEmailsDocumentAsync();
            if (doc == null || !doc.EmailsToSendList.Any())
                return false;

            var activeSurveys = doc.EmailsToSendList
                .Where(s => s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
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
                    Email = e
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

                var subject = $"Survey Invitation: {entry.SurveyName}";
                var body = $"Hello,<br/><br/>Please complete the survey <b>{entry.SurveyName}</b> for teacher feedback.<br/><br/>  https://witty-beach-0b0c08903.2.azurestaticapps.net";

                using var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);
                _logger.LogInformation("Sent email to {Email} for survey {SurveyName}", entry.Email, entry.SurveyName);

            }

            // remove sent emails from the document
            foreach (var e in batch)
            {
                var surveyBatch = doc.EmailsToSendList.FirstOrDefault(s => s.SurveyId == e.SurveyId);
                if (surveyBatch != null)
                    surveyBatch.Emails.Remove(e.Email);

                if ( surveyBatch!=null && !surveyBatch.Emails.Any())
                {
                    doc.EmailsToSendList.Remove(surveyBatch);
                }
            }

            // clean up expired surveys if any remain
            var expired = doc.EmailsToSendList
                .Where(s => s.EndDate < DateTime.UtcNow)
                .ToList();

            foreach (var survey in expired)
            {
                doc.EmailsToSendList.Remove(survey);
                _logger.LogInformation("Removed expired survey {SurveyName} ({SurveyId})", survey.SurveyName, survey.SurveyId);
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
}
