namespace FeedBackApp.Core.Email.Constants;

/// <summary>
/// Constants related to email sending configuration and limits.
/// </summary>
public static class EmailConstants
{
    /// <summary>
    /// Maximum number of emails that can be sent in a single batch per day.
    /// </summary>
    public const short DailyEmailLimit = 500;

    /// <summary>
    /// Default SMTP port for email sending.
    /// </summary>
    public const short DefaultSmtpPort = 587;

    /// <summary>
    /// Default SMTP host for Gmail.
    /// </summary>
    public const string DefaultSmtpHost = "smtp.gmail.com";

    /// <summary>
    /// Frontend application URL for survey links.
    /// </summary>
    public const string FrontendUrl = "https://witty-beach-0b0c08903.2.azurestaticapps.net";
}


