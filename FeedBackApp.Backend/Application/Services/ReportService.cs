using Application.Exceptions;
using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ReportService(IReportRepository repository,
                               IBlobContext blob,
                               ILogger<ReportService> reportLogger) : IReportService
    {
        private readonly IReportRepository _repository = repository;
        private readonly IBlobContext _blob = blob;
        private readonly ILogger<ReportService> _logger = reportLogger;

        public async Task CompileAndStore(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                throw new ArgumentException("TemplateId must be provided.", nameof(templateId));

            try
            {
                _logger.LogInformation("Report compilation started.");
                await _repository.CompileAndStoreEvaluationReports(templateId);
                _logger.LogInformation("Report compilation finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during report compilation.");
                throw new ReportCompilationException(
                    $"Report compilation failed for templateId={templateId}. See inner exception.",
                    ex
                );
            }
        }

        public async Task<byte[]> DownloadAdminAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            try
            {
                _logger.LogInformation("Downloading admin report: {File}", fileName);
                var bytes = await _blob.DownloadAdminAsync(fileName);
                _logger.LogInformation("Admin report downloaded: {File} ({Len} bytes)", fileName, bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download admin report: {File}", fileName);
                throw new ReportStorageException($"Failed to download admin report '{fileName}'.", ex);
            }
        }

        public async Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName)
        {
            if (string.IsNullOrWhiteSpace(teacherEmail))
                throw new ArgumentException("Teacher email must be provided.", nameof(teacherEmail));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            try
            {
                _logger.LogInformation("Downloading teacher report: {Email} / {File}", teacherEmail, fileName);
                var bytes = await _blob.DownloadTeacherAsync(teacherEmail, fileName);
                _logger.LogInformation("Teacher report downloaded: {Email} / {File} ({Len} bytes)",
                    teacherEmail, fileName, bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download teacher report: {Email} / {File}", teacherEmail, fileName);
                throw new ReportStorageException(
                    $"Failed to download teacher report '{fileName}' for '{teacherEmail}'.", ex);
            }
        }
    }
}
