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
    /// Creates an Email entity from survey metadata for the specified role.
    /// </summary>
    /// <param name="metadata">Survey metadata containing dates and title.</param>
    /// <param name="surveyId">The survey identifier.</param>
    /// <param name="role">The recipient role (Teacher or Admin).</param>
    /// <param name="recipientEmails">List of email addresses for the recipients. For Teacher role, this is extracted from metadata.</param>
    /// <returns>Configured CoreEmail instance.</returns>
    public static CoreEmail CreateEmail(SurveyMetadata metadata, Guid surveyId, Role role, List<string> recipientEmails)
    {
        return new CoreEmail
        {
            Emails = recipientEmails,
            StartDate = metadata.StartDate,
            EndDate = metadata.EndDate,
            Role = role,
            SurveyId = surveyId.ToString(),
            SurveyName = metadata.Title
        };
    }

    /// <summary>
    /// Creates an Email entity for teachers from survey metadata.
    /// </summary>
    public static CoreEmail CreateTeacherEmail(SurveyMetadata metadata, Guid surveyId)
    {
        var teachers = metadata.Teachers
            .Where(t => !string.IsNullOrWhiteSpace(t.Email))
            .Select(t => t.Email!)
            .ToList();

        return CreateEmail(metadata, surveyId, Role.Teacher, teachers);
    }

    /// <summary>
    /// Creates an Email entity for admin/leaders from survey metadata.
    /// </summary>
    public static CoreEmail CreateAdminEmail(SurveyMetadata metadata, Guid surveyId, string leaderEmails)
    {
        var leadersEmails = leaderEmails
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .ToList();

        return CreateEmail(metadata, surveyId, Role.Admin, leadersEmails);
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

