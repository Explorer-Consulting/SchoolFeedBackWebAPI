using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ReportService(IReportRepository repository, ILogger<ReportService> logger) : IReportService
    {
        private readonly IReportRepository _repository = repository;
        private readonly ILogger<ReportService> _logger = logger;

        public async Task CompileAndStore(string id)
        {
            await _repository.CompileAndStoreEvaluationReports(id);
            _logger.LogInformation("Compilation of reports in ReportService.....");
        }
    }
}
