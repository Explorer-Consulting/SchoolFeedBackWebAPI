namespace FeedBackApp.Backend.Infrastructure.Configuration
{
    public sealed class AuthorizationOptions
    {
        public const string UniversalStudentSetId = "everyone";
        public string AdminEmails { get; set; } = "";
        public bool RequireStudentWhiteList { get; set; } = true;
        public bool UseUniversalStudentGroup { get; set; } = false;
    }
}
