using System.Linq;

namespace FeedBackApp.Core.Model;

/// <summary>
/// Represents the root document containing all pending emails to be sent.
/// This document is stored in Cosmos DB and serves as a queue for email notifications.
/// </summary>
public class EmailsToSend
{
    /// <summary>
    /// Gets or sets the unique identifier for this document in Cosmos DB.
    /// Typically set to a constant value like "emailsToSend".
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of email entries grouped by survey and role.
    /// Each entry contains a list of recipient email addresses for a specific survey.
    /// </summary>
    public IList<Email> EmailsToSendList { get; set; } = new List<Email>();

    /// <summary>
    /// Checks if there are any email entries in the list.
    /// </summary>
    /// <returns>True if the list contains at least one email entry.</returns>
    public bool HasEmails()
    {
        return EmailsToSendList.Count > 0;
    }

    /// <summary>
    /// Gets the total count of all recipient email addresses across all entries.
    /// </summary>
    /// <returns>The total number of email addresses to be sent.</returns>
    public int GetTotalEmailCount()
    {
        return EmailsToSendList.Sum(e => e.Emails.Count);
    }
}
