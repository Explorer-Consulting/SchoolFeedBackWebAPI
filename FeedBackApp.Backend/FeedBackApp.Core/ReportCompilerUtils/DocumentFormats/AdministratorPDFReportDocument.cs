using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Administrator (global) report document in PDF format.
    /// <para>
    /// The purpose of this document is to present aggregated statistics
    /// for all teachers and all questions in one place.
    /// The content is composed from the components listed in
    /// <see cref="ReportDocument.ReportComponents"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Create an instance of <see cref="AdministratorPDFReportDocument"/> with the required <see cref="ReportMetadata"/>.</item>
    /// <item>Add the report components to be displayed to <see cref="ReportDocument.ReportComponents"/>.</item>
    /// <item>Call <see cref="RenderDocument"/> to generate the PDF.</item>
    /// </list>
    /// </remarks>
    public sealed class AdministratorPDFReportDocument(ReportMetadata metadata, Recipient? recipient = null)
        : ReportDocument(metadata, recipient), IDocument
    {
        /// <summary>
        /// Composes the PDF pages (A4 size, 28pt margins, with header and content).
        /// </summary>
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);

                page.Header().Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Text($"Globális Felmérés - {Metadata.InstitutionName}")
                        .FontSize(14).Bold();
                    col.Item().Text("Aggregált statisztikai felmérés a tanárok között")
                        .FontSize(11).FontColor(Colors.Grey.Darken2);
                });
                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    foreach (var component in ReportComponents)
                        col.Item()
                           .PreventPageBreak()
                           .Component(component);
                });
            });
        }

        /// <summary>
        /// Renders the PDF document into memory and returns the byte array.
        /// </summary>
        /// <returns>The generated PDF content as a byte array.</returns>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            this.GeneratePdf(ms);

            Data = ms.ToArray();
            return Task.FromResult(Data);
        }
    }
}
