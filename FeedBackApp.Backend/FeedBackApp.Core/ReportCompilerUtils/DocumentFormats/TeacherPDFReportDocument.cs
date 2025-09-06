using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    public sealed class TeacherPDFReportDocument(ReportMetadata metadata, Recipient recipient)
        : ReportDocument(metadata, recipient), IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    foreach (var component in ReportComponents)
                        col.Item().Component(component);
                });
            });
        }

        public override byte[] RenderDocument()
        {
            using var ms = new MemoryStream();
            this.GeneratePdf(ms);

            // a dokumentumok kis meretuek lesznek foleg az Excel-ek, a PDF-ek lesznek nagyobbak de nagyon max 350-400 KB, ezert igy hagytam es nem streamelem oket egybol Blob-ba.
            Data = ms.ToArray();
            return Data;
        }
    }
}
