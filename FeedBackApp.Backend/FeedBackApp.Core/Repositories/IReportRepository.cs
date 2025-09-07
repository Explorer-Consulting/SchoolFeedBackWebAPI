namespace FeedBackApp.Core.Repositories
{
    public interface IReportRepository
    {
        Task CompileAndStoreEvaluationReports(string questionTemplateID);
    }
}
