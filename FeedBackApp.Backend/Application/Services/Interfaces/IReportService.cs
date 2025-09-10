namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        Task CompileAndStore(string id);
        Task<byte[]> DownloadAdminAsync(string surveyId);
        Task<byte[]> DownloadTeacherAsync(string teacherEmail, string surveyId);
    }
}
