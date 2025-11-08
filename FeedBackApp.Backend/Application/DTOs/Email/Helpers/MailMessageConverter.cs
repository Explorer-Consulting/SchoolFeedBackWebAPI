using Application.Email.Models;
using System.Net.Mail;

namespace Application.Email.Helpers;

/// <summary>
/// Helper class for converting EmailMessage to System.Net.Mail.MailMessage.
/// This is a temporary adapter until we replace SmtpClient with a better solution.
/// </summary>
public static class MailMessageConverter
{
    /// <summary>
    /// Converts an EmailMessage to a System.Net.Mail.MailMessage.
    /// </summary>
    public static MailMessage ToMailMessage(EmailMessage emailMessage, string fromAddress, string fromName)
    {
        var from = new MailAddress(fromAddress, fromName);
        var to = new MailAddress(emailMessage.To);

        var message = new MailMessage(from, to)
        {
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsBodyHtml = emailMessage.IsHtml
        };

        foreach (var attachment in emailMessage.Attachments)
        {
            var stream = new MemoryStream(attachment.Data);
            stream.Position = 0;
            var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
            message.Attachments.Add(mailAttachment);
        }

        return message;
    }
}

