using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    public sealed class TeacherPDFReportDocument(ReportMetadata metadata, Recipient recipient) : ReportDocument(metadata, recipient), IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            throw new NotImplementedException();
        }
        public override async Task<byte[]> RenderDocument()
        {
            return [];
        }
    }
}
