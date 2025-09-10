using Application.Exceptions;
using Application.Services.Interfaces;
using Azure.Storage.Blobs.Models;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// Application-level service that compiles evaluation reports and retrieves stored report files
    /// (admin and teacher) from Azure Blob Storage via <see cref="IBlobContext"/>.
    /// </summary>
    public class ReportService(IReportRepository repository,
                               IBlobContext blob,
                               ILogger<ReportService> reportLogger) : IReportService
    {
        private readonly IReportRepository _repository = repository;
        private readonly IBlobContext _blob = blob;
        private readonly ILogger<ReportService> _logger = reportLogger;

        // -------------------- Report compilation --------------------

        /// <summary>
        /// Triggers report compilation for the given template and stores the results in Blob Storage.
        /// </summary>
        /// <param name="templateId">The questionnaire template ID used to compile reports.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="templateId"/> is null or whitespace.</exception>
        /// <exception cref="ReportCompilationException">
        /// Wrapped exception when report compilation fails. See inner exception for details.
        /// </exception>
        public async Task CompileAndStore(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                throw new ArgumentException("TemplateId must be provided.", nameof(templateId));

            try
            {
                _logger.LogInformation("Report compilation started for templateId={TemplateId}.", templateId);
                await _repository.CompileAndStoreEvaluationReports(templateId).ConfigureAwait(false);
                _logger.LogInformation("Report compilation finished for templateId={TemplateId}.", templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during report compilation for templateId={TemplateId}.", templateId);
                throw new ReportCompilationException(
                    $"Report compilation failed for templateId={templateId}. See inner exception.",
                    ex
                );
            }
        }

        // -------------------- Admin reports --------------------

        /// <summary>
        /// Downloads the admin-level PDF report for the given survey ID.
        /// </summary>
        /// <param name="surveyId">The survey (report batch) identifier.</param>
        /// <returns>The file content as a byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="surveyId"/> is null or whitespace.</exception>
        /// <exception cref="ReportStorageException">Thrown when the blob cannot be downloaded.</exception>
        public async Task<byte[]> DownloadAdminAsync(string surveyId)
        {
            if (string.IsNullOrWhiteSpace(surveyId))
                throw new ArgumentException("SurveyId must be provided.", nameof(surveyId));

            var fileName = $"{surveyId}_global_report.pdf";
            try
            {
                _logger.LogInformation("Downloading admin report: {File}", fileName);
                var bytes = await _blob.DownloadAdminAsync(fileName).ConfigureAwait(false);
                _logger.LogInformation("Admin report downloaded: {File} ({Len} bytes)", fileName, bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download admin report: {File}", fileName);
                throw new ReportStorageException($"Failed to download admin report '{fileName}'.", ex);
            }
        }

        /// <summary>
        /// Lists admin files whose names start with the given ID prefix (server-side prefix filtering).
        /// Useful for retrieving all admin artifacts for a specific survey (e.g., PDF, XLSX).
        /// </summary>
        /// <param name="idPrefix">The file name prefix (typically the survey ID).</param>
        /// <returns>A read-only list of matching file names (without the "admin/" prefix).</returns>
        /// <exception cref="ReportStorageException">Thrown on listing errors.</exception>
        public async Task<IReadOnlyList<string>> ListAdminFileNamesByIdPrefixAsync(string idPrefix)
        {
            var list = new List<string>();
            try
            {
                await foreach (BlobItem item in _blob.FindAdminFilesByIdPrefixAsync(idPrefix))
                {
                    const string basePrefix = "admin/";
                    var nameOnly = item.Name.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase)
                        ? item.Name[basePrefix.Length..]
                        : item.Name;
                    list.Add(nameOnly);
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list admin files with prefix {Prefix}", idPrefix);
                throw new ReportStorageException($"Failed to list admin files with id prefix '{idPrefix}'.", ex);
            }
        }

        /// <summary>
        /// Downloads all admin files that start with the given ID prefix and returns (fileName, bytes) pairs.
        /// </summary>
        /// <param name="idPrefix">The file name prefix (typically the survey ID).</param>
        /// <returns>A read-only list of tuples containing the file name and binary content.</returns>
        /// <exception cref="ReportStorageException">Thrown when a download fails.</exception>
        public async Task<IReadOnlyList<(string FileName, byte[] Data)>> DownloadAdminFilesByIdPrefixAsync(string idPrefix)
        {
            var results = new List<(string FileName, byte[] Data)>();
            try
            {
                await foreach (var item in _blob.FindAdminFilesByIdPrefixAsync(idPrefix))
                {
                    const string basePrefix = "admin/";
                    var nameOnly = item.Name.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase)
                        ? item.Name[basePrefix.Length..]
                        : item.Name;

                    var bytes = await _blob.DownloadAdminAsync(nameOnly).ConfigureAwait(false);
                    results.Add((nameOnly, bytes));
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download admin files with prefix {Prefix}", idPrefix);
                throw new ReportStorageException($"Failed to download admin files with id prefix '{idPrefix}'.", ex);
            }
        }

        // -------------------- Teacher reports --------------------

        /// <summary>
        /// Downloads a teacher report. If <paramref name="subject"/> is provided, downloads that exact file.
        /// Otherwise, fetches all files by survey ID prefix and returns the first match.
        /// </summary>
        /// <param name="teacherEmail">Teacher's email (folder under "teachers/").</param>
        /// <param name="surveyId">Survey ID used as the leading part of file names.</param>
        /// <param name="subject">
        /// Optional subject token included in the file name. When provided, the method downloads
        /// <c>{surveyId}_{teacherEmail}_{subject}_report.pdf</c>. When omitted, the method searches
        /// for all files by <paramref name="surveyId"/> prefix and returns the first.
        /// </param>
        /// <returns>The file content as a byte array.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="teacherEmail"/> or <paramref name="surveyId"/> is null or whitespace.
        /// </exception>
        /// <exception cref="FileNotFoundException">Thrown when no file is found for the given survey ID (when subject is null).</exception>
        /// <exception cref="ReportStorageException">Thrown on underlying storage errors.</exception>
        public async Task<byte[]> DownloadTeacherAsync(string teacherEmail, string surveyId, string? subject = null)
        {
            if (string.IsNullOrWhiteSpace(teacherEmail))
                throw new ArgumentException("Teacher email must be provided.", nameof(teacherEmail));
            if (string.IsNullOrWhiteSpace(surveyId))
                throw new ArgumentException("SurveyId must be provided.", nameof(surveyId));

            try
            {
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    // Exact file by full name
                    var fileName = $"{surveyId}_{teacherEmail}_{subject}_report.pdf";
                    _logger.LogInformation("Downloading teacher report: {Email} / {File}", teacherEmail, fileName);
                    var bytes = await _blob.DownloadTeacherAsync(teacherEmail, fileName).ConfigureAwait(false);
                    _logger.LogInformation("Teacher report downloaded: {Email} / {File} ({Len} bytes)",
                        teacherEmail, fileName, bytes.Length);
                    return bytes;
                }
                else
                {
                    // No subject → download first match by surveyId prefix
                    var files = await DownloadTeacherFilesByIdPrefixAsync(teacherEmail, surveyId);
                    if (files.Count == 0)
                        throw new FileNotFoundException($"No teacher reports found for {teacherEmail} and surveyId={surveyId}.");

                    return files[0].Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download teacher report(s) for {Email} and surveyId={SurveyId}", teacherEmail, surveyId);
                throw new ReportStorageException(
                    $"Failed to download teacher report(s) for '{teacherEmail}' and surveyId={surveyId}.", ex);
            }
        }

        /// <summary>
        /// Lists teacher file names under "teachers/{teacherEmail}/" that start with the given ID prefix.
        /// </summary>
        /// <param name="teacherEmail">Teacher's email (folder under "teachers/").</param>
        /// <param name="idPrefix">The file name prefix (typically the survey ID).</param>
        /// <returns>A read-only list of matching file names (without the folder prefix).</returns>
        /// <exception cref="ReportStorageException">Thrown on listing errors.</exception>
        public async Task<IReadOnlyList<string>> ListTeacherFileNamesByIdPrefixAsync(string teacherEmail, string idPrefix)
        {
            var list = new List<string>();
            try
            {
                await foreach (BlobItem item in _blob.FindTeacherFilesByIdPrefixAsync(teacherEmail, idPrefix))
                {
                    var basePrefix = $"teachers/{teacherEmail.ToLowerInvariant().Trim()}/";
                    var nameOnly = item.Name.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase)
                        ? item.Name[basePrefix.Length..]
                        : item.Name;
                    list.Add(nameOnly);
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list teacher files for {Email} with prefix {Prefix}", teacherEmail, idPrefix);
                throw new ReportStorageException($"Failed to list teacher files for '{teacherEmail}' with id prefix '{idPrefix}'.", ex);
            }
        }

        /// <summary>
        /// Downloads all teacher files under "teachers/{teacherEmail}/" that start with the given ID prefix.
        /// Returns (fileName, bytes) pairs.
        /// </summary>
        /// <param name="teacherEmail">Teacher's email (folder under "teachers/").</param>
        /// <param name="idPrefix">The file name prefix (typically the survey ID).</param>
        /// <returns>A read-only list of tuples containing the file name and binary content.</returns>
        /// <exception cref="ReportStorageException">Thrown when a download fails.</exception>
        public async Task<IReadOnlyList<(string FileName, byte[] Data)>> DownloadTeacherFilesByIdPrefixAsync(
            string teacherEmail, string idPrefix)
        {
            var results = new List<(string FileName, byte[] Data)>();
            try
            {
                await foreach (var item in _blob.FindTeacherFilesByIdPrefixAsync(teacherEmail, idPrefix))
                {
                    var basePrefix = $"teachers/{teacherEmail.ToLowerInvariant().Trim()}/";
                    var nameOnly = item.Name.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase)
                        ? item.Name[basePrefix.Length..]
                        : item.Name;

                    var bytes = await _blob.DownloadTeacherAsync(teacherEmail, nameOnly).ConfigureAwait(false);
                    results.Add((nameOnly, bytes));
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download teacher files for {Email} with prefix {Prefix}", teacherEmail, idPrefix);
                throw new ReportStorageException($"Failed to download teacher files for '{teacherEmail}' with id prefix '{idPrefix}'.", ex);
            }
        }
    }
}
