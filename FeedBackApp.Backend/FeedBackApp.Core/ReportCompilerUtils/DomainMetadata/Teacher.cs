namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    public sealed record Teacher(string EmailAddress, string SubjectName) : Recipient(EmailAddress);
}
