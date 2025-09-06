using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;

namespace FeedBackApp.Core.Repositories
{
    public interface IReportRepository
    {
        Task CompileAndStoreEvaluationReports(/*implementation-dependent*/);
        Task DeleteEvaluationReport(string id);
        Task DeleteAllEvaluationReports(/*implementation-dependent*/);
        Task<ReportDocument> RetrieveEvaluationReport(string id);

        [Obsolete("Can be used for optional mechanics")]
        IAsyncEnumerable<ReportDocument> RetrieveAllEvaluationReports(/*implementation-dependent*/);
    }
}
