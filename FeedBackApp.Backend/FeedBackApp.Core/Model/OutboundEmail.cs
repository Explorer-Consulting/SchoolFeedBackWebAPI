using FeedBackApp.Core.Model;
public class OutboundEmail
{
    public string Id { get; set; } = $"email_{Guid.NewGuid():D}";
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string HtmlBody { get; set; } = default!;
    public string Type { get; set; } = "SelfOptIn";
    public string? CampaignId { get; set; }          // for reporting
    public string Status { get; set; } = "Pending";  // Pending/Sent/Failed
    public int Attempt { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}