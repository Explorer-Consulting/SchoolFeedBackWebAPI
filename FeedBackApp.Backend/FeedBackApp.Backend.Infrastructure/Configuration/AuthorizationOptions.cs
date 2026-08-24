namespace FeedBackApp.Backend.Infrastructure.Configuration
{
    public sealed class AuthorizationOptions
    {
        public string AdminEmails { get; set; } = "";
        public bool RequireStudentWhiteList { get; set; } = true;
    }
}
