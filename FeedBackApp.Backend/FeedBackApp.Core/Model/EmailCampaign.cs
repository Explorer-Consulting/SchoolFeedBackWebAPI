namespace FeedBackApp.Core.Model;

public class EmailCampaign
{
    public string Id { get; set; } = $"emailcampaign_{Guid.NewGuid():D}";
    public string TemplateStorageId { get; set; } = "";    // "questiontemplates_<guid>"
    public Guid TemplateGuid { get; set; }                 // from route
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";            // admin email
    public string Subject { get; set; } = "";
    public string BaseUrl { get; set; } = "";              // public base
    public bool Personalized { get; set; }                 // token includes email claim
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Queued { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
}