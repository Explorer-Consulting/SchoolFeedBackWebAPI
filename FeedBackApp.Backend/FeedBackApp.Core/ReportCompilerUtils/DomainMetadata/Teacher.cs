namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{

    // itt majd lesz olyan hogy FirstName, LastName
    // az email az az egyedi azonosito
    public sealed record Teacher(string EmailAddress, string SubjectName) : Recipient(EmailAddress);
}
