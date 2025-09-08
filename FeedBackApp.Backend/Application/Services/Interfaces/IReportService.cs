namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        Task CompileAndStore(string id);
    }
}
