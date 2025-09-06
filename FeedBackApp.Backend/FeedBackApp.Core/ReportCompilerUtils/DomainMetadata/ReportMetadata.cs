public sealed class ReportMetadata
{
    public required string MimeType { get; init; }
    public required string FileName { get; init; }
    public string Encoding { get; init; } = "UTF-8";
    public string DatabaseIdentifier { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreationDate { get; init; } = DateTime.Now;
    public required string Author { get; init; }
    public required string BLOB_URI { get; init; }
}
