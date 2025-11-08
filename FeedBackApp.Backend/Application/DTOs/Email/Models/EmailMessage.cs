namespace Application.Email.Models;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Recipient email address.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content (HTML supported).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the body contains HTML content.
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// List of email attachments.
    /// </summary>
    public List<EmailAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// Represents an email attachment.
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Attachment file data.
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Attachment file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type of the attachment.
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";
}

