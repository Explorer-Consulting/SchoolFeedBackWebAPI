using Application.Email.Constants;

namespace Application.Email.Configuration;

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
            FromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") 
                ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not set."),
            FromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") 
                ?? throw new InvalidOperationException("EMAIL_FROM_NAME is not set."),
            AppPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") 
                ?? throw new InvalidOperationException("EMAIL_APP_PASSWORD is not set."),
            LeaderEmails = Environment.GetEnvironmentVariable("LeaderEmail") ?? string.Empty
        };
    }
}

