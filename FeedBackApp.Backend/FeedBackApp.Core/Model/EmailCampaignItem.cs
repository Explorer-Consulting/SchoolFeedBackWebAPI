namespace FeedBackApp.Core.Model;

public class EmailCampaignItem
{
    public string Id { get; set; } = $"emailitem_{Guid.NewGuid():D}";
    public string CampaignId { get; set; } = "";
    public string RecipientEmail { get; set; } = "";
    public string? RecipientName { get; set; }
    public string Status { get; set; } = "queued";         // queued/sent/failed/skipped
    public string? Error { get; set; }
    public DateTimeOffset EnqueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int Attempt { get; set; } = 0;
}