using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using CoreEmail = FeedBackApp.Core.Model.Email;

namespace Application.Email.Helpers;

/// <summary>
/// Helper class for compiling email lists from survey metadata.
/// </summary>
public static class EmailCompilationHelper
{
    /// <summary>
    /// Creates an Email entity for teachers from survey metadata.
    /// </summary>
    public static CoreEmail CreateTeacherEmail(SurveyMetadata metadata, Guid surveyId)
    {
        var teachers = metadata.Teachers
            .Where(t => !string.IsNullOrWhiteSpace(t.Email))
            .Select(t => t.Email)
            .ToList();

        return new CoreEmail
        {
            Emails = teachers,
            StartDate = metadata.StartDate,
            EndDate = metadata.EndDate,
            Role = Role.Teacher,
            SurveyId = surveyId.ToString(),
            SurveyName = metadata.Title
        };
    }

    /// <summary>
    /// Creates an Email entity for admin/leaders from survey metadata.
    /// </summary>
    public static CoreEmail CreateAdminEmail(SurveyMetadata metadata, Guid surveyId, string leaderEmails)
    {
        var leadersEmails = leaderEmails
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new CoreEmail
        {
            Emails = leadersEmails,
            StartDate = metadata.StartDate,
            EndDate = metadata.EndDate,
            Role = Role.Admin,
            SurveyId = surveyId.ToString(),
            SurveyName = metadata.Title
        };
    }

    /// <summary>
    /// Ensures an EmailsToSend document exists, creating one if needed.
    /// </summary>
    public static EmailsToSend EnsureEmailDocument(EmailsToSend? existingDocument)
    {
        return existingDocument ?? new EmailsToSend
        {
            EmailsToSendList = new List<CoreEmail>()
        };
    }
}

