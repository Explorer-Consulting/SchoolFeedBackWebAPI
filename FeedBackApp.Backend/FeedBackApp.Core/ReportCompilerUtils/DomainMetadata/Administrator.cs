namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    // itt majd lesz olyan, hogy FirstName, LastName
    public sealed record Administrator(string EmailAddress) : Recipient(EmailAddress);
}
