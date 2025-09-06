using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    public abstract class ReportDocument(ReportMetadata metadata, Recipient recipient)
    {
        // the addressee
        public Recipient Recipient { get; init; } = recipient;
        // document metadata
        public ReportMetadata Metadata { get; init; } = metadata;
        //ebbe lesz benne maga renderelt doksi;

        // the content of the document
        public byte[] Data { get; set; } = [];

        // the pdf components that the document contains
        public List<IComponent> ReportComponents { get; set; } = [];

        //function to render the document
        public abstract byte[] RenderDocument();
    }
}
