using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Tanári riport dokumentum (PDF formátumban).
    /// <para>
    /// Ez az osztály felelős egy adott tanárhoz tartozó PDF-riport
    /// összeállításáért. A dokumentum fejlécében megjelennek a tanár alapadatai
    /// (email cím és tantárgy), a tartalomban pedig a <see cref="ReportComponents"/>
    /// listában definiált riportkomponensek.
    /// </para>
    /// <para>
    /// A QuestPDF könyvtár <see cref="IDocument"/> interfészét implementálja.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Hozz létre egy <see cref="TeacherPDFReportDocument"/> példányt a
    /// szükséges <see cref="ReportMetadata"/> és <see cref="Recipient"/> adatokkal.</item>
    /// <item>Add hozzá a kívánt riportkomponenseket a <see cref="ReportComponents"/> listához.</item>
    /// <item>Hívd meg a <see cref="RenderDocument"/> metódust a PDF generálásához.</item>
    /// </list>
    /// </remarks>
    public sealed class TeacherPDFReportDocument(ReportMetadata metadata, Recipient recipient)
        : ReportDocument(metadata, recipient), IDocument
    {
        /// <summary>
        /// A PDF dokumentum tartalmának összeállítása.
        /// <para>
        /// Az oldal mérete A4, margó 28 pont. A fejlécben megjelenik a tanár email címe
        /// és a tantárgy neve, a tartalomban pedig a hozzáadott riportkomponensek.
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

                        col.Item().Text($"Tanár (Email): {teacher.EmailAddress}")
                            .FontSize(12).Bold();

                        col.Item().Text($"Tantárgy: {teacher.SubjectName}")
                            .FontSize(11).FontColor(Colors.Grey.Darken2);
                    });
                }

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    foreach (var component in ReportComponents)
                        col.Item().Component(component);
                });
            });
        }

        /// <summary>
        /// A PDF dokumentum legenerálása és bináris formátumban való visszaadása.
        /// <para>
        /// A metódus a dokumentumot memóriába rendereli, majd a kész bájt tömböt
        /// eltárolja a <see cref="ReportDocument.Data"/> property-ben és visszaadja.
        /// </para>
        /// </summary>
        /// <returns>A kész PDF dokumentum tartalma bájt tömbként.</returns>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            this.GeneratePdf(ms);

            Data = ms.ToArray();
            return Task.FromResult(Data);
        }
    }
}
