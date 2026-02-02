namespace FeedBackApp.Core.Repositories;

public interface IEmailRenderer
{
    (string subject, string htmlBody) BuildOptInMail(
        string subject,
        string displayName,
        string link,
        DateTimeOffset expiresAtUtc,
        string templateTitle);
}