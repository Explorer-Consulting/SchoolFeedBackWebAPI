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
public sealed class EmailRepository(AppDBContext context, ILogger<EmailRepository>? logger = null) : IEmailRepository
{
    private readonly AppDBContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<EmailRepository>? _logger = logger;
    
    /// <summary>
    /// The unique identifier for the EmailsToSend document in Cosmos DB.
    /// This document contains all pending emails to be sent.
    /// </summary>
    private const string EmailsToSendDocumentId = "emailsToSend";

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
                .FirstOrDefaultAsync(e => e.Id == EmailsToSendDocumentId);

            if (document == null)
            {
                _logger?.LogDebug(
                    "EmailsToSend document not found with ID: {DocumentId}",
                    EmailsToSendDocumentId);
            }
            else
            {
                _logger?.LogDebug(
                    "Retrieved EmailsToSend document with ID: {DocumentId}. Email count: {EmailCount}",
                    EmailsToSendDocumentId,
                    document.EmailsToSendList?.Count ?? 0);
            }

            return document;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error retrieving EmailsToSend document with ID: {DocumentId}. Error: {ErrorMessage}",
                EmailsToSendDocumentId,
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
                doc.Id = EmailsToSendDocumentId;
                _logger?.LogDebug(
                    "Set document ID to: {DocumentId}",
                    EmailsToSendDocumentId);
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
            // This exception is thrown when optimistic concurrency control detects a conflict.
            // The document may have been modified by another process.
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
