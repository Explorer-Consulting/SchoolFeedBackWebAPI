using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface
{
    public interface IBlobContext
    {
        Task UploadAdminAsync(string fileName, byte[] data, string contentType);
        Task UploadTeacherAsync(string teacherEmail, string fileName, byte[] data, string contentType);

        Task<byte[]> DownloadAdminAsync(string fileName);
        Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName);
    }
}
