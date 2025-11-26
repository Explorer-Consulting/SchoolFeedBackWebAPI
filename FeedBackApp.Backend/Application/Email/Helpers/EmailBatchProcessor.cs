using Application.Email.Models;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using CoreEmail = FeedBackApp.Core.Model.Email;

namespace Application.Email.Helpers;

/// <summary>
/// Helper class for processing email batches from the email repository.
/// </summary>
public static class EmailBatchProcessor
{
    /// <summary>
    /// Cleans up expired student surveys from the email document.
    /// </summary>
    public static void RemoveExpiredSurveys(EmailsToSend emailDocument, DateTime currentTime)
    {
        var expired = emailDocument.EmailsToSendList
            .Where(s => s.EndDate < currentTime && s.Role == Role.Student)
            .ToList();

        foreach (var survey in expired)
        {
            emailDocument.EmailsToSendList.Remove(survey);
        }
    }

    /// <summary>
    /// Gets active surveys that are ready to send.
    /// </summary>
    public static List<CoreEmail> GetActiveSurveys(EmailsToSend emailDocument, DateTime currentTime)
    {
        return emailDocument.EmailsToSendList
            .Where(s => s.StartDate <= currentTime)
            .ToList();
    }

    /// <summary>
    /// Creates a batch of email entries from active surveys, respecting the daily limit.
    /// </summary>
    public static List<EmailBatchEntry> CreateBatch(List<CoreEmail> activeSurveys, short dailyLimit)
    {
        return activeSurveys
            .SelectMany(s => s.Emails.Select(e => new EmailBatchEntry
            {
                SurveyId = s.SurveyId,
                SurveyName = s.SurveyName,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Email = e,
                Role = s.Role
            }))
            .Take(dailyLimit)
            .ToList();
    }

    /// <summary>
    /// Removes sent emails from the email document.
    /// </summary>
    public static void RemoveSentEmails(EmailsToSend emailDocument, List<EmailBatchEntry> sentBatch)
    {
        foreach (var entry in sentBatch)
        {
            var surveyBatch = emailDocument.EmailsToSendList.FirstOrDefault(s => s.SurveyId == entry.SurveyId);
            if (surveyBatch != null)
            {
                surveyBatch.Emails.Remove(entry.Email);

                if (!surveyBatch.Emails.Any())
                {
                    emailDocument.EmailsToSendList.Remove(surveyBatch);
                }
            }
        }
    }
}

