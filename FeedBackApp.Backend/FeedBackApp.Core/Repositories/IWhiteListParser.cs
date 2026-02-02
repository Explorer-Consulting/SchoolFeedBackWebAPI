namespace FeedBackApp.Core.Repositories;

public interface IWhitelistParser
{
    Task<IReadOnlyList<WhitelistRow>> ParseCsvAsync(Stream csv);
}