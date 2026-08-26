using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Teacher report document (in PDF format).
    /// <para>
    /// This class is responsible for assembling a PDF report
    /// for a specific teacher. The header of the document displays
    /// the teacher’s basic information (email address and subject),
    /// while the content is composed of the report components
    /// defined in <see cref="ReportComponents"/>.
    /// </para>
    /// <para>
    /// Implements the <see cref="IDocument"/> interface from the QuestPDF library.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Create a <see cref="TeacherPDFReportDocument"/> instance with the required
    /// <see cref="ReportMetadata"/> and <see cref="Recipient"/> data.</item>
    /// <item>Add the desired report components to the <see cref="ReportComponents"/> list.</item>
    /// <item>Call <see cref="RenderDocument"/> to generate the PDF.</item>
    /// </list>
    /// </remarks>
    public sealed class TeacherPDFReportDocument(ReportMetadata metadata, Recipient recipient)
        : ReportDocument(metadata, recipient), IDocument
    {
        /// <summary>
        /// Composes the content of the PDF document.
        /// <para>
        /// The page size is A4 with a 28pt margin. The header displays the teacher’s
        /// email address and subject name, and the content includes the added report components.
        /// </para>
        /// </summary>
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);

                if (Recipient is Teacher teacher)
                {
                    page.Header().Column(col =>
                    {
                        col.Spacing(4);
                        col.Item().Text(Metadata.InstitutionName)
                            .FontSize(12).Bold();
                        col.Item().Text($"Tanár E-mail: {teacher.EmailAddress}")
                            .FontSize(12).Bold();

                        col.Item().Text($"Tantárgy: {teacher.SubjectName}")
                            .FontSize(11).FontColor(Colors.Grey.Darken2);
                    });
                }

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    foreach (var component in ReportComponents)
                    {
                        col.Item()
                           .PreventPageBreak()
                           .Component(component);
                    }
                });
            });
        }

        /// <summary>
        /// Generates the PDF document and returns it in binary format.
        /// <para>
        /// The method renders the document into memory, then stores the resulting byte array
        /// in <see cref="ReportDocument.Data"/> and returns it.
        /// </para>
        /// </summary>
        /// <returns>The generated PDF document as a byte array.</returns>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            this.GeneratePdf(ms);

            Data = ms.ToArray();
            return Task.FromResult(Data);
        }
    }
}
