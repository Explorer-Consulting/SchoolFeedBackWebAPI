using FeedBackApp.Core.Email.Constants;

namespace FeedBackApp.Core.Email.Configuration;

/// <summary>
/// Configuration settings for email sending.
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// Email address to send emails from.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the sender.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Application password for SMTP authentication.
    /// </summary>
    public string AppPassword { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server host.
    /// </summary>
    public string SmtpHost { get; set; } = EmailConstants.DefaultSmtpHost;

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public short SmtpPort { get; set; } = EmailConstants.DefaultSmtpPort;

    /// <summary>
    /// Comma-separated list of leader/admin email addresses.
    /// </summary>
    public string LeaderEmails { get; set; } = string.Empty;

    /// <summary>
    /// Creates an EmailConfiguration instance from environment variables.
    /// </summary>
    /// <returns>Configured EmailConfiguration instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are not set.</exception>
    public static EmailConfiguration FromEnvironment()
    {
        return new EmailConfiguration
        {
            FromAddress = Environment.GetEnvironmentVariable("Email:FromAddress") 
                ?? throw new InvalidOperationException("Email:FromAddress is not set."),
            FromName = Environment.GetEnvironmentVariable("Email:FromName") 
                ?? throw new InvalidOperationException("Email:FromName is not set."),
            AppPassword = Environment.GetEnvironmentVariable("Email:AppPassword") 
                ?? throw new InvalidOperationException("Email:AppPassword is not set."),
            LeaderEmails = Environment.GetEnvironmentVariable("AdminEmails") ?? string.Empty
        };
    }
}

