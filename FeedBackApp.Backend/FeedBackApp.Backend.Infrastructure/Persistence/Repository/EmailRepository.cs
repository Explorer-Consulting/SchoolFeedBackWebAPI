using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository;

/// <summary>
/// Repository implementation for managing email documents in Cosmos DB.
/// Handles retrieval and updates of the EmailsToSend document which contains
/// all pending emails to be sent to students, teachers, and administrators.
/// </summary>
public sealed class EmailRepository : IEmailRepository
{
    private readonly AppDBContext _context;
    private readonly ILogger<EmailRepository>? _logger;

    /// <summary>
    /// Initializes a new instance of the EmailRepository.
    /// </summary>
    /// <param name="context">The database context for Cosmos DB operations.</param>
    /// <param name="logger">Optional logger for tracking repository operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public EmailRepository(AppDBContext context, ILogger<EmailRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the EmailsToSend document from Cosmos DB.
    /// </summary>
    /// <returns>
    /// The EmailsToSend document if found, null otherwise.
    /// Returns null if no document exists with the configured ID.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a database operation fails or the context is disposed.
    /// </exception>
    public async Task<EmailsToSend?> GetEmailsDocumentAsync()
    {
        try
        {
            var document = await _context.EmailsToSend
                .FirstOrDefaultAsync(e => e.Id == EmailRepositoryConstants.EmailsToSendDocumentId);

            if (document == null)
            {
                _logger?.LogDebug(
                    "EmailsToSend document not found with ID: {DocumentId}",
                    EmailRepositoryConstants.EmailsToSendDocumentId);
            }
            else
            {
                _logger?.LogDebug(
                    "Retrieved EmailsToSend document with ID: {DocumentId}. Email count: {EmailCount}",
                    EmailRepositoryConstants.EmailsToSendDocumentId,
                    document.EmailsToSendList?.Count ?? 0);
            }

            return document;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error retrieving EmailsToSend document with ID: {DocumentId}. Error: {ErrorMessage}",
                EmailRepositoryConstants.EmailsToSendDocumentId,
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates or creates the EmailsToSend document in Cosmos DB.
    /// </summary>
    /// <param name="doc">The EmailsToSend document to update. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when doc is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a database operation fails, the context is disposed, or save changes fails.
    /// </exception>
    public async Task UpdateEmailsDocumentAsync(EmailsToSend doc)
    {
        if (doc == null)
        {
            throw new ArgumentNullException(nameof(doc), "EmailsToSend document cannot be null.");
        }

        try
        {
            // Ensure the document has the correct ID
            if (string.IsNullOrWhiteSpace(doc.Id))
            {
                doc.Id = EmailRepositoryConstants.EmailsToSendDocumentId;
                _logger?.LogDebug(
                    "Set document ID to: {DocumentId}",
                    EmailRepositoryConstants.EmailsToSendDocumentId);
            }

            _context.EmailsToSend.Update(doc);
            
            var changesSaved = await _context.SaveChangesAsync();
            
            _logger?.LogInformation(
                "Successfully updated EmailsToSend document with ID: {DocumentId}. Changes saved: {ChangesSaved}. Email entries: {EmailCount}",
                doc.Id,
                changesSaved,
                doc.EmailsToSendList?.Count ?? 0);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger?.LogError(
                ex,
                "Concurrency conflict while updating EmailsToSend document with ID: {DocumentId}. The document may have been modified by another process.",
                doc.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger?.LogError(
                ex,
                "Database update error while updating EmailsToSend document with ID: {DocumentId}. Error: {ErrorMessage}",
                doc.Id,
                ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Unexpected error while updating EmailsToSend document with ID: {DocumentId}. Error: {ErrorMessage}",
                doc.Id,
                ex.Message);
            throw;
        }
    }
}
