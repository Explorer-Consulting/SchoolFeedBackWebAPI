using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Többválasztós (Multiple Choice) kérdésekhez tartozó riportkomponens.
    /// <para>
    /// Megjeleníti a kérdés szövegét, az összes válasz számát, az opciók eloszlását
    /// mini sávdiagrammal és táblázattal, továbbá – ha rendelkezésre áll – a leggyakoribb
    /// együtt-előfordulásokat (opció-párok).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Példányosítsd a komponenst <see cref="MultipleChoiceEvaluationData"/> adattal.</item>
    /// <item>Add hozzá a dokumentum <c>ReportComponents</c> listájához.</item>
    /// <item>A dokumentum generálásakor a komponens a tartalom megfelelő részébe renderelődik.</item>
    /// </list>
    /// Előfeltételek:
    /// <list type="bullet">
    /// <item><see cref="MultipleChoiceEvaluationData.AnswerOptions"/> az opciók listája (szöveg).</item>
    /// <item><see cref="MultipleChoiceEvaluationData.Frequencies"/> az abszolút gyakoriságok (opció → db).</item>
    /// <item><see cref="MultipleChoiceEvaluationData.RelativeFrequenciesPercent"/> opcionális relatív százalékok.</item>
    /// </list>
    /// </remarks>
    public sealed class MultipleChoiceReportComponent(MultipleChoiceEvaluationData dataSource)
        : ReportComponent<MultipleChoiceEvaluationData>(dataSource)
    {
        // --- Design tokenek ---
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float MetaSize = 10f;
        private const float TextSize = 11.5f;
        private const float BarHeight = 8f;
        private const float BarWidth = 180f;

        // --- Színek ---
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string TrackGrey = Colors.Grey.Lighten3;
        private static readonly string AccentBlue = Colors.Blue.Medium;
        private static readonly string PageWhite = Colors.White;

        /// <summary>
        /// A komponens megjelenítésének leírása.
        /// <para>
        /// Szekciók:
        /// 1) Címsor (kérdés),
        /// 2) Üres állapot (hiányzó opciók vagy válaszok),
        /// 3) Meta (összes válasz),
        /// 4) Eloszlás táblázat mini sávdiagramokkal,
        /// 5) Opcionális: top együtt-előfordulások (A–B párok).
        /// </para>
        /// </summary>
        /// <param name="container">A QuestPDF konténer, amelybe a komponens renderel.</param>
        public override void Compose(IContainer container)
        {
            var data = DataSource;

            // Bemenetek biztonságos kezelése
            var options = data.AnswerOptions.IsDefaultOrEmpty ? [] : data.AnswerOptions;
            var answers = data.Answers.IsDefaultOrEmpty ? [] : data.Answers;

            int n = answers.Length; // összes szavazat (rekord)

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // Cím
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // Üres állapotok
                    if (options.Length == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("Ehhez a kérdéshez nincsenek opciók megadva.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    if (n == 0 || data.Frequencies is null || data.Frequencies.Count == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("Ehhez a kérdéshez nem érkezett érvényes válasz.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    // Meta – összes válasz
                    col.Item().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                        t.Span("Válaszok száma: ");
                        t.Span(n.ToString(CultureInfo.InvariantCulture)).SemiBold();
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // Eloszlás tábla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);     // opció szöveg
                            c.RelativeColumn(4);     // mini-sáv + %
                            c.ConstantColumn(50);    // darab
                        });

                        // Fejléc
                        table.Cell().PaddingBottom(4).Text("Opció").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).Text("Eloszlás").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).AlignRight().Text("Db").FontSize(MetaSize).FontColor(MetaGrey);

                        // Sorok – gyakoriság szerint csökkenő
                        var freq = data.Frequencies;
                        var rel = data.RelativeFrequenciesPercent;

                        foreach (var (option, count) in freq.OrderByDescending(kv => kv.Value))
                        {
                            double pct = 0.0;
                            if (rel != null && rel.TryGetValue(option, out var rpct))
                                pct = rpct;
                            else if (n > 0)
                                pct = (double)count / n * 100.0;

                            float filled = (float)(BarWidth * (pct / 100.0));

                            // 1) Opció szöveg
                            table.Cell().PaddingVertical(6)
                                .Text(option)
                                .FontSize(TextSize).FontColor(TextBlack);

                            // 2) Mini-sáv + százalék
                            table.Cell().PaddingVertical(6).Row(row =>
                            {
                                row.AutoItem()
                                   .Width(BarWidth)
                                   .Height(BarHeight)
                                   .Background(TrackGrey)
                                   .Border(0.5f).BorderColor(SubtleGrey)
                                   .Column(cc =>
                                   {
                                       cc.Item()
                                         .Width(filled)
                                         .Height(BarHeight)
                                         .Background(AccentBlue);
                                   });

                                row.AutoItem().PaddingLeft(8)
                                   .Text(pct.ToString("0.#", CultureInfo.InvariantCulture) + "%")
                                       .FontSize(MetaSize)
                                       .FontColor(MetaGrey);
                            });

                            // 3) Darabszám
                            table.Cell().PaddingVertical(6).AlignRight()
                                .Text(count.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // Top együtt-előfordulások (opcionális)
                    if (data.Cooccurrences is not null && data.Cooccurrences.Count > 0)
                    {
                        var topPairs = data.Cooccurrences
                            .OrderByDescending(kv => kv.Value)
                            .Take(5)
                            .ToList();

                        col.Item().Text("Leggyakoribb együtt-előfordulások")
                            .FontSize(MetaSize).FontColor(MetaGrey);

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.ConstantColumn(60);
                            });

                            t.Cell().PaddingBottom(4).Text("A").FontSize(MetaSize).FontColor(MetaGrey);
                            t.Cell().PaddingBottom(4).Text("B").FontSize(MetaSize).FontColor(MetaGrey);
                            t.Cell().PaddingBottom(4).AlignRight().Text("Db").FontSize(MetaSize).FontColor(MetaGrey);

                            foreach (var ((A, B), cnt) in topPairs)
                            {
                                t.Cell().PaddingVertical(4).Text(A).FontSize(TextSize).FontColor(TextBlack);
                                t.Cell().PaddingVertical(4).Text(B).FontSize(TextSize).FontColor(TextBlack);
                                t.Cell().PaddingVertical(4).AlignRight().Text(cnt.ToString(CultureInfo.InvariantCulture))
                                    .FontSize(TextSize).FontColor(TextBlack);
                            }
                        });
                    }
                });
        }
    }
}
