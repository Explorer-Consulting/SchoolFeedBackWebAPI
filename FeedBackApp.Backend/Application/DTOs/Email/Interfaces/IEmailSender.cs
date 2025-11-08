using Application.Email.Models;

namespace Application.Email.Interfaces;

/// <summary>
/// Interface for sending email messages asynchronously.
/// This abstraction allows for different email sending implementations (SMTP, SendGrid, etc.).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

