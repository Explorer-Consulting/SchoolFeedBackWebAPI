using Application.Exceptions;
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
            try
            {
                _logger.LogInformation("Report compilation started for templateId={Id}", id);
                await _repository.CompileAndStoreEvaluationReports(id);
                _logger.LogInformation("Report compilation finished for templateId={Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during report compilation for templateId={Id}", id);
                throw new ReportCompilationException(
                    id,
                    $"Report compilation failed for templateId={id}. See inner exception.",
                    ex
                );
            }
        }
    }
}
