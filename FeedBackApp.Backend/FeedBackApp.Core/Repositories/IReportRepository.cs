using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;

namespace FeedBackApp.Core.Repositories
{
    public interface IReportRepository
    {
        Task CompileAndStoreEvaluationReports(/*implementation-dependent*/);
    }
}
