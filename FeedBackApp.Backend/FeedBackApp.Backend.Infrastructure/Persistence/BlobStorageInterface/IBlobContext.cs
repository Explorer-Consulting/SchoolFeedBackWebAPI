using Azure.Storage.Blobs.Models;

namespace FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface
{
    /// <summary>
    /// Abstraction layer for accessing and managing report files in Azure Blob Storage.
    /// Provides methods to upload, download, list, and search teacher/admin reports.
    /// </summary>
    public interface IBlobContext
    {
        // -------------------- Upload --------------------

        /// <summary>
        /// Uploads an admin report to the "admin/" folder in Blob Storage.
        /// </summary>
        /// <param name="fileName">Name of the file (e.g., "report123.pdf").</param>
        /// <param name="data">Binary content of the file.</param>
        /// <param name="contentType">MIME type of the file (e.g., "application/pdf").</param>
        Task UploadAdminAsync(string fileName, byte[] data, string contentType);

        /// <summary>
        /// Uploads a teacher report to the "teachers/{teacherEmail}/" folder.
        /// </summary>
        /// <param name="teacherEmail">Teacher’s email address, used as folder name.</param>
        /// <param name="fileName">Name of the file (e.g., "survey123_teacher@school.com_math_report.pdf").</param>
        /// <param name="data">Binary content of the file.</param>
        /// <param name="contentType">MIME type of the file.</param>
        Task UploadTeacherAsync(string teacherEmail, string fileName, byte[] data, string contentType);

        // -------------------- Download --------------------

        /// <summary>
        /// Downloads a single admin report from the "admin/" folder.
        /// </summary>
        /// <param name="fileName">The file name (without the "admin/" prefix).</param>
        /// <returns>Binary content of the file.</returns>
        Task<byte[]> DownloadAdminAsync(string fileName);

        /// <summary>
        /// Downloads a single teacher report from the "teachers/{teacherEmail}/" folder.
        /// </summary>
        /// <param name="teacherEmail">Teacher’s email address.</param>
        /// <param name="fileName">The file name (without the folder prefix).</param>
        /// <returns>Binary content of the file.</returns>
        Task<byte[]> DownloadTeacherAsync(string teacherEmail, string fileName);

        // -------------------- Listing --------------------

        /// <summary>
        /// Lists all teacher folders under "teachers/". 
        /// Each result is the prefix path, e.g., "teachers/teacher@school.com".
        /// </summary>
        IAsyncEnumerable<string> ListTeacherFoldersAsync();

        /// <summary>
        /// Lists all report files for a given teacher.
        /// </summary>
        /// <param name="teacherEmail">Teacher’s email address.</param>
        /// <returns>A sequence of <see cref="BlobItem"/> objects with file metadata.</returns>
        IAsyncEnumerable<BlobItem> ListTeacherFilesAsync(string teacherEmail);

        /// <summary>
        /// Lists all report files in the "admin/" folder.
        /// </summary>
        /// <returns>A sequence of <see cref="BlobItem"/> objects with file metadata.</returns>
        IAsyncEnumerable<BlobItem> ListAdminFilesAsync();

        // -------------------- Search by ID prefix --------------------

        /// <summary>
        /// Finds all teacher report files for a given survey ID prefix.
        /// Uses server-side filtering to return only files whose names start with the given prefix.
        /// </summary>
        /// <param name="teacherEmail">Teacher’s email address.</param>
        /// <param name="idPrefix">The ID prefix to match at the beginning of file names.</param>
        /// <returns>A sequence of <see cref="BlobItem"/> objects that match the prefix.</returns>
        IAsyncEnumerable<BlobItem> FindTeacherFilesByIdPrefixAsync(string teacherEmail, string idPrefix);

        /// <summary>
        /// Finds all admin report files for a given survey ID prefix.
        /// </summary>
        /// <param name="idPrefix">The ID prefix to match at the beginning of file names.</param>
        /// <returns>A sequence of <see cref="BlobItem"/> objects that match the prefix.</returns>
        IAsyncEnumerable<BlobItem> FindAdminFilesByIdPrefixAsync(string idPrefix);
    }
}
