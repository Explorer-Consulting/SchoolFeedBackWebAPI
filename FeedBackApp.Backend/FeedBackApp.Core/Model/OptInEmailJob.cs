namespace FeedBackApp.Core.Model;

public record OptInEmailJob(
    string CampaignId,
    Guid TemplateGuid,
    string TemplateStorageId,
    string RecipientEmail,
    string? RecipientName,
    DateTimeOffset ExpiresAtUtc,
    string Subject,
    string BaseUrl,
    bool Personalized
    );

