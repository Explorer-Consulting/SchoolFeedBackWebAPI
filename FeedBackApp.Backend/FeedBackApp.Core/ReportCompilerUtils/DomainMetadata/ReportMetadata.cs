namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    public sealed class ReportMetadata
    {
        public required string MimeType { get; init; }
        public required string FileName { get; init; }
        public required string Author { get; init; }
        public required string BLOB_URI { get; set; }
    }
}
