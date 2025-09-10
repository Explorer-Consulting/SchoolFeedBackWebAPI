using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Context
{

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

        public Task UploadAdminAsync(string fileName, byte[] data, string contentType) =>
            UploadAsync(_container.GetBlobClient($"admin/{fileName}"), data, contentType);

        public Task UploadTeacherAsync(string teacherEmail, string fileName, byte[] data, string contentType) =>
            UploadAsync(_container.GetBlobClient($"teachers/{San(teacherEmail)}/{fileName}"), data, contentType);

        public async Task<byte[]> DownloadAdminAsync(string fileName)
        {
            var res = await _container.GetBlobClient($"admin/{fileName}").DownloadContentAsync();
            return res.Value.Content.ToArray();
        }

        public async Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName)
        {
            var res = await _container.GetBlobClient($"teachers/{San(teacherEmail)}/{fileName}").DownloadContentAsync();
            return res.Value.Content.ToArray();
        }

        private static async Task UploadAsync(BlobClient blob, byte[] data, string? contentType)
        {
            // 1) Feltöltés felülírással
            using var ms = new MemoryStream(data, writable: false);
            await blob.UploadAsync(ms, overwrite: true);

            // 2) Content-Type beállítása
            var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = ct });
        }

        private static string San(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            Span<char> bad = ['/', '\\', '?', '#', '%', '+', '\t', '\r', '\n', ':', '*', '"', '<', '>', '|'];
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input) sb.Append(bad.Contains(ch) ? '-' : ch);
            return sb.ToString().Trim();
        }
    }
}
