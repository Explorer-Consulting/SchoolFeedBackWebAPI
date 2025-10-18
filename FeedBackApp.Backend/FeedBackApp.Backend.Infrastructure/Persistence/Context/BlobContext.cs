using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Context
{
    /// <summary>
    /// Concrete implementation of <see cref="IBlobContext"/> that manages report files
    /// in Azure Blob Storage under the "admin/" and "teachers/{email}/" folders.
    /// </summary>
    public sealed class BlobContext : IBlobContext
    {
        private readonly BlobContainerClient _container;

        public BlobContext(BlobServiceClient service, string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("AZURE_REPORTS_CONTAINER is not set.");

            _container = service.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists(PublicAccessType.None);
        }

        // -------------------- Upload --------------------

        /// <summary>
        /// Uploads an admin report file to the "admin/" folder.
        /// Overwrites the file if it already exists.
        /// </summary>
        public Task UploadAdminAsync(string fileName, byte[] data, string contentType) =>
            UploadAsync(_container.GetBlobClient($"admin/{fileName}"), data, contentType);

        /// <summary>
        /// Uploads a teacher report file to "teachers/{teacherEmail}/".
        /// Overwrites the file if it already exists.
        /// </summary>
        public Task UploadTeacherAsync(string teacherEmail, string fileName, byte[] data, string contentType) =>
            UploadAsync(_container.GetBlobClient($"teachers/{San(teacherEmail)}/{fileName}"), data, contentType);

        // -------------------- Download --------------------

        /// <summary>
        /// Downloads a single admin report as a byte array.
        /// </summary>
        public async Task<byte[]> DownloadAdminAsync(string fileName)
        {
            var blob = _container.GetBlobClient($"admin/{fileName}");
            using var ms = new MemoryStream();
            await blob.DownloadToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        /// <summary>
        /// Downloads a single teacher report as a byte array.
        /// </summary>
        public async Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName)
        {
            var blob = _container.GetBlobClient($"teachers/{San(teacherEmail)}/{fileName}");
            using var ms = new MemoryStream();
            await blob.DownloadToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        // -------------------- Listing --------------------

        /// <summary>
        /// Lists all teacher folders under "teachers/".
        /// Each result is a folder path like "teachers/teacher@school.com".
        /// </summary>
        public async IAsyncEnumerable<string> ListTeacherFoldersAsync()
        {
            await foreach (var item in _container.GetBlobsByHierarchyAsync(prefix: "teachers/", delimiter: "/"))
            {
                if (item.IsPrefix && item.Prefix is not null && item.Prefix != "teachers/")
                    yield return item.Prefix.TrimEnd('/');
            }
        }

        /// <summary>
        /// Lists all report files for a specific teacher.
        /// Returns BlobItem objects containing metadata (name, size, last modified).
        /// </summary>
        public async IAsyncEnumerable<BlobItem> ListTeacherFilesAsync(string teacherEmail)
        {
            var prefix = $"teachers/{San(teacherEmail)}/";
            await foreach (var item in _container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/"))
                if (item.IsBlob && item.Blob is not null)
                    yield return item.Blob;
        }

        /// <summary>
        /// Lists all report files stored in the "admin/" folder.
        /// Returns BlobItem objects containing metadata (name, size, last modified).
        /// </summary>
        public IAsyncEnumerable<BlobItem> ListAdminFilesAsync()
            => _container.GetBlobsAsync(prefix: "admin/");

        // -------------------- Find by ID prefix --------------------

        /// <summary>
        /// Finds all teacher files for a given teacher whose names start with the provided ID prefix.
        /// Useful for retrieving all reports generated for a specific survey.
        /// </summary>
        public IAsyncEnumerable<BlobItem> FindTeacherFilesByIdPrefixAsync(string teacherEmail, string idPrefix)
        {
            var prefix = $"teachers/{San(teacherEmail)}/{idPrefix}";
            return _container.GetBlobsAsync(prefix: prefix);
        }

        /// <summary>
        /// Finds all admin files whose names start with the provided ID prefix.
        /// Useful for retrieving all admin-level reports for a survey.
        /// </summary>
        public IAsyncEnumerable<BlobItem> FindAdminFilesByIdPrefixAsync(string idPrefix)
            => _container.GetBlobsAsync(prefix: $"admin/{idPrefix}");

        // -------------------- Helpers --------------------

        /// <summary>
        /// Sanitizes the email string so it can safely be used as part of a blob path.
        /// Converts to lowercase and replaces illegal characters with underscores.
        /// </summary>
        private static string San(string email)
        {
            var s = email.Trim().ToLowerInvariant();
            return s
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace('#', '_')
                .Replace('?', '_');
        }

        // Internal helper for uploads
        private static async Task UploadAsync(BlobClient blob, byte[] data, string? contentType)
        {
            using var ms = new MemoryStream(data, writable: false);
            await blob.UploadAsync(ms, overwrite: true).ConfigureAwait(false);

            var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = ct }).ConfigureAwait(false);
        }
    }
}
