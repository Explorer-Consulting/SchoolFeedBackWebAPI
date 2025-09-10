namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    public sealed record Administrator(string EmailAddress) : Recipient(EmailAddress);
}
