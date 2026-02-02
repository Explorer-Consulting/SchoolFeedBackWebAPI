using FeedBackApp.Core.Model;

namespace FeedBackApp.Core.Repositories;

public interface IBulkOptInService
{
    // creates EmailCampaign, persists items
    Task<EmailCampaign> StartCampaignAsync(
        Guid templateGuid,
        string templateStorageId,
        string subject,
        string baseUrl,
        bool personalized,
        DateTimeOffset expiresAtUtc,
        IEnumerable<WhitelistRow> recipients,
        string createdBy);
}

