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

    /*
     I. Constants Explanation: constants are usually declared in configuration files like local.settings.json
        or appsettings.json. However, in this case, the constant is specific to the EmailRepository's
        internal logic and is not expected to change based on deployment environments or configurations.
        It is unnecessary and unpractical to keep it in a seperate class, furthermore it is a business of database storage.
        Do not use like this. For just trying it out it's ok, but it is not a proper set-up.
     
     */
}


