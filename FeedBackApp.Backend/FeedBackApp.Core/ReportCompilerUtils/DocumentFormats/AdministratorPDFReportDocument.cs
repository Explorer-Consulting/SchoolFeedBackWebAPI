using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Adminisztrátori (globális) riport dokumentum PDF formátumban.
    /// <para>
    /// A dokumentum célja, hogy az összes tanárra és kérdésre vonatkozó
    /// összesített statisztikákat egy helyen jelenítse meg.
    /// A tartalom a <see cref="ReportDocument.ReportComponents"/> listában
    /// szereplő komponensekből áll.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Hozz létre egy <see cref="AdministratorPDFReportDocument"/> példányt a szükséges <see cref="ReportMetadata"/>-val.</item>
    /// <item>Add hozzá a megjelenítendő riportkomponenseket a <see cref="ReportDocument.ReportComponents"/> listához.</item>
    /// <item>Hívd meg a <see cref="RenderDocument"/> metódust a PDF generálásához.</item>
    /// </list>
    /// </remarks>
    public sealed class AdministratorPDFReportDocument(ReportMetadata metadata, Recipient? recipient = null)
        : ReportDocument(metadata, recipient), IDocument
    {
        /// <summary>
        /// A PDF oldalainak felépítése (A4-es oldal, 28 pontos margó, fejléccel és tartalommal).
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
                    col.Item().Text("Globális riport")
                        .FontSize(14).Bold();
                    col.Item().Text("Összesített statisztikai jelentés az összes tanárhoz")
                        .FontSize(11).FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    foreach (var component in ReportComponents)
                        col.Item().Component(component);
                });
            });
        }

        /// <summary>
        /// A PDF dokumentum renderelése memóriába és a bájt tömb visszaadása.
        /// </summary>
        /// <returns>A legenerált PDF tartalma bájt tömbként.</returns>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            this.GeneratePdf(ms);

            Data = ms.ToArray();
            return Task.FromResult(Data);
        }
    }
}
