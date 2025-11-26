namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository;

/// <summary>
/// Constants used by the EmailRepository for document identification and operations.
/// </summary>
internal static class EmailRepositoryConstants
{
    /// <summary>
    /// The unique identifier for the EmailsToSend document in Cosmos DB.
    /// This document contains all pending emails to be sent.
    /// </summary>
    public const string EmailsToSendDocumentId = "emailsToSend";
}


