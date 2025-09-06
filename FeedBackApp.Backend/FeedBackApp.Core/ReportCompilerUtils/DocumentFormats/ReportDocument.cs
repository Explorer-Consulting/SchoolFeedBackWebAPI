using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Absztrakt bázisosztály minden riport dokumentumhoz.
    /// <para>
    /// A <see cref="ReportDocument"/> reprezentál egy általános riportot,
    /// amely rendelkezik metaadatokkal, címzett információval, tartalommal
    /// és riportkomponensekkel. A konkrét dokumentumtípusokat (pl. PDF, Excel)
    /// a leszármazott osztályok valósítják meg.
    /// </para>
    /// </summary>
    public abstract class ReportDocument(ReportMetadata metadata, Recipient? recipient)
    {
        /// <summary>
        /// A dokumentum címzettje (pl. <see cref="Teacher"/>).
        /// <para>
        /// Lehet <c>null</c>, ha a riport nem kötődik konkrét személyhez
        /// (pl. globális adminisztrátori riport).
        /// </para>
        /// </summary>
        public Recipient? Recipient { get; init; } = recipient;

        /// <summary>
        /// A dokumentumhoz tartozó metaadatok (szerző, fájlnév, MIME-típus, URI stb.).
        /// </summary>
        public ReportMetadata Metadata { get; init; } = metadata;

        /// <summary>
        /// A legenerált dokumentum bináris tartalma.
        /// <para>
        /// A <see cref="RenderDocument"/> meghívása után kerül feltöltésre.
        /// Alapértelmezés szerint üres bájt tömb (<c>[]</c>).
        /// </para>
        /// </summary>
        public byte[] Data { get; set; } = [];

        /// <summary>
        /// A riporthoz tartozó komponensek listája.
        /// <para>
        /// PDF esetén ezek QuestPDF <see cref="IComponent"/> implementációk,
        /// amelyek a dokumentum tartalmát építik fel.
        /// </para>
        /// </summary>
        public List<IComponent> ReportComponents { get; set; } = [];

        /// <summary>
        /// A dokumentum legenerálása bináris formátumban.
        /// <para>
        /// A konkrét implementációt a leszármazott osztályok határozzák meg
        /// (pl. PDF export, Excel export).
        /// </para>
        /// </summary>
        /// <returns>A kész dokumentum tartalma bájt tömbként.</returns>
        public abstract byte[] RenderDocument();
    }
}
