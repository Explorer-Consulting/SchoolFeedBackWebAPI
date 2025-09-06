using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    public sealed class AdministratorPDFReportDocument(ReportMetadata metadata, Recipient recipient) : ReportDocument(metadata, recipient), IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            throw new NotImplementedException();
        }

        public override byte[] RenderDocument()
        {
            throw new NotImplementedException();
        }
    }
}
