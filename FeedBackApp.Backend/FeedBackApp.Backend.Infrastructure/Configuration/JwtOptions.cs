namespace FeedBackApp.Backend.Infrastructure.Configuration
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public int TokenTtlMinutes { get; set; }
    }
}
