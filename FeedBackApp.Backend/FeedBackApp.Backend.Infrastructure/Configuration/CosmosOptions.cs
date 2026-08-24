namespace FeedBackApp.Backend.Infrastructure.Configuration
{
    public sealed class CosmosOptions
    {
        public string AccountEndpoint { get; set; } = "";
        public string AccountKey { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string ContainerName { get; set; } = "";
    }
}
