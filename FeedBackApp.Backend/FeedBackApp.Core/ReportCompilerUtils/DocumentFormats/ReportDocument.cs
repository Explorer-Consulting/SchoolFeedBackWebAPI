using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Abstract base class for all report documents.
    /// <para>
    /// <see cref="ReportDocument"/> represents a general report
    /// that contains metadata, recipient information, content,
    /// and report components. Specific document types (e.g. PDF, Excel)
    /// are implemented by derived classes.
    /// </para>
    /// </summary>
    public abstract class ReportDocument(ReportMetadata metadata, Recipient? recipient)
    {
        /// <summary>
        /// The recipient of the document (e.g. <see cref="Teacher"/>).
        /// <para>
        /// May be <c>null</c> if the report is not tied to a specific person
        /// (e.g. a global administrator report).
        /// </para>
        /// </summary>
        public Recipient? Recipient { get; init; } = recipient;

        /// <summary>
        /// Metadata associated with the document (author, filename, MIME type, URI, etc.).
        /// </summary>
        public ReportMetadata Metadata { get; init; } = metadata;

        /// <summary>
        /// The binary content of the generated document.
        /// <para>
        /// Populated after calling <see cref="RenderDocument"/>.
        /// Defaults to an empty byte array (<c>[]</c>).
        /// </para>
        /// </summary>
        public byte[] Data { get; set; } = [];

        /// <summary>
        /// The list of components associated with the report.
        /// <para>
        /// In PDF documents these are QuestPDF <see cref="IComponent"/> implementations
        /// that build up the document content.
        /// </para>
        /// </summary>
        public List<IComponent> ReportComponents { get; set; } = [];

        /// <summary>
        /// Generates the document in binary format.
        /// <para>
        /// The actual implementation is defined by the derived classes
        /// (e.g. PDF export, Excel export).
        /// </para>
        /// </summary>
        /// <returns>The generated document content as a byte array.</returns>
        public abstract Task<byte[]> RenderDocument();
    }
}
