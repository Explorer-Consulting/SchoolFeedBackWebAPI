namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        Task CompileAndStore(string templateId);

        // Admin riportok
        Task<byte[]> DownloadAdminAsync(string surveyId);
        Task<IReadOnlyList<string>> ListAdminFileNamesByIdPrefixAsync(string idPrefix);
        Task<IReadOnlyList<(string FileName, byte[] Data)>> DownloadAdminFilesByIdPrefixAsync(string idPrefix);

        Task<byte[]> DownloadTeacherAsync(string teacherEmail, string surveyId, string? subject = null);
        Task<IReadOnlyList<string>> ListTeacherFileNamesByIdPrefixAsync(string teacherEmail, string idPrefix);
        Task<IReadOnlyList<(string FileName, byte[] Data)>> DownloadTeacherFilesByIdPrefixAsync(string teacherEmail, string idPrefix);
    }
}
