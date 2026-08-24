namespace FeedBackApp.Backend.Infrastructure.Configuration
{
    public sealed class MicrosoftAuthOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string TenantId { get; set; } = "common";
    }
}
