using FeedBackApp.Core.Email;
using FeedBackApp.Core.Email.Configuration;
using FeedBackApp.Core.Email.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace FeedBackApp.Backend.Infrastructure.Email;

/// <summary>
/// MailKit-based implementation of IEmailSender for sending emails via SMTP.
/// This replaces the deprecated System.Net.Mail.SmtpClient with a modern, async-first library.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{

    /*
     =====================================================================================================
     use primary constructor and properties.
     =====================================================================================================
     */
    private readonly EmailConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    /// <summary>
    /// Initializes a new instance of the SmtpEmailSender.
    /// </summary>
    /// <param name="configuration">Email configuration settings.</param>
    /// <param name="logger">Logger for tracking email operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or logger is null.</exception>
    public SmtpEmailSender(EmailConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an email message asynchronously using MailKit SMTP client.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully, false otherwise.</returns>
    public async Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {

        /*
         ======================================================================================================
         It is a good practice for ChatGPT to add CancellationToken for function parameters. It is not bad but in this case it's unnecessary, and not handled properly.
        ======================================================================================================
         */
        if (message == null) /*=============== use gurads for simple null checks =============================*/
        {
            _logger.LogError("Cannot send email: message is null");
            return false;
        }

        if (string.IsNullOrWhiteSpace(message.To))
        {
            _logger.LogError("Cannot send email: recipient address is null or empty");
            return false;
        }

        try
        {
            var mimeMessage = CreateMimeMessage(message);
            
            using var client = new SmtpClient();
            
            // Connect to SMTP server
            await client.ConnectAsync(
                _configuration.SmtpHost, 
                _configuration.SmtpPort, 
                SecureSocketOptions.StartTls, 
                cancellationToken);

            // Authenticate
            await client.AuthenticateAsync(
                _configuration.FromAddress, 
                _configuration.AppPassword, 
                cancellationToken);

            // Send email
            await client.SendAsync(mimeMessage, cancellationToken);
            
            // Disconnect
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent email to {Recipient} with subject: {Subject}",
                message.To,
                message.Subject);

            return true;
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(
                ex,
                "SMTP command error while sending email to {Recipient}. StatusCode: {StatusCode}, Response: {Response}",
                message.To,
                ex.StatusCode,
                ex.Message);
            return false;
        }
        catch (SmtpProtocolException ex)
        {
            _logger.LogError(
                ex,
                "SMTP protocol error while sending email to {Recipient}. Error: {ErrorMessage}",
                message.To,
                ex.Message);
            return false;
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError(
                ex,
                "SMTP authentication failed while sending email to {Recipient}. Error: {ErrorMessage}",
                message.To,
                ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while sending email to {Recipient}. Error: {ErrorMessage}",
                message.To,
                ex.Message);
            return false;
        }
    }
    /*
     ======================================================================================================
    U return from the catch blocks but u do not handle the possible exceptions.
     ======================================================================================================
     */

    /// <summary>
    /// Creates a MimeMessage from an EmailMessage.
    /// </summary>
    private MimeMessage CreateMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();
        
        // From
        mimeMessage.From.Add(new MailboxAddress(_configuration.FromName, _configuration.FromAddress));
        
        // To
        mimeMessage.To.Add(new MailboxAddress(string.Empty, message.To));
        
        // Subject
        mimeMessage.Subject = message.Subject;
        
        // Body
        var bodyBuilder = new BodyBuilder();
        if (message.IsHtml) 
        {
            bodyBuilder.HtmlBody = message.Body;
        }
        else
        {
            bodyBuilder.TextBody = message.Body;
        }
        
        // Attachments
        foreach (var attachment in message.Attachments)
        {
            if (attachment.Data.Length > 0 && !string.IsNullOrWhiteSpace(attachment.FileName))
            {
                var contentType = ContentType.Parse(attachment.ContentType);
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Data, contentType);
                /*we will not use files like attachments, we will send links with time*/
            }
        }
        
        mimeMessage.Body = bodyBuilder.ToMessageBody();
        
        return mimeMessage;
    }
}

