namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        Task CompileAndStore(string id);
        Task<byte[]> DownloadAdminAsync(string fileName);
        Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName);
    }
}
