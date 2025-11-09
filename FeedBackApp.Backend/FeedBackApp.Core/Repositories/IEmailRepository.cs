using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories;

/// <summary>
/// Repository interface for managing email documents in persistent storage.
/// Provides operations to retrieve and update the EmailsToSend document which contains
/// all pending email notifications queued for delivery.
/// </summary>
public interface IEmailRepository
{
    /// <summary>
    /// Retrieves the EmailsToSend document from persistent storage.
    /// </summary>
    /// <returns>
    /// The EmailsToSend document if found, null if no document exists.
    /// The document contains all pending email entries grouped by survey and role.
    /// </returns>
    /// <remarks>
    /// This method is used by the email service to retrieve pending emails for batch processing.
    /// If no document exists, it indicates there are no emails queued for sending.
    /// </remarks>
    Task<EmailsToSend?> GetEmailsDocumentAsync();

    /// <summary>
    /// Updates or creates the EmailsToSend document in persistent storage.
    /// </summary>
    /// <param name="doc">The EmailsToSend document to persist. Must not be null.</param>
    /// <remarks>
    /// This method is called after email batch processing to remove sent emails,
    /// or when new email entries are added to the queue (e.g., after report compilation).
    /// The document ID should be set to the configured constant value.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when doc is null.</exception>
    Task UpdateEmailsDocumentAsync(EmailsToSend doc);
}
