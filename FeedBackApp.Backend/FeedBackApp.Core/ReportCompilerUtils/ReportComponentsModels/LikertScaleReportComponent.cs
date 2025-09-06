using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Likert-skálás kérdéshez tartozó riportkomponens.
    /// <para>
    /// Megjeleníti a kérdés szövegét, a mintanagyságot, alapvető leíró statisztikákat
    /// (átlag, medián, szórás), az értékek eloszlását mini sávdiagrammal és táblázattal,
    /// továbbá részletes mutatókat (minimum, maximum, módusz, elégedettségi index, egyetértési arány).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Példányosítsd a komponenst egy <see cref="LikertScaleEvaluationData"/> adattal.</item>
    /// <item>Add a komponenst egy dokumentum <c>ReportComponents</c> listájához.</item>
    /// <item>A dokumentum generálásakor a komponens a tartalom megfelelő részébe renderelődik.</item>
    /// </list>
    /// </remarks>
    public sealed class LikertScaleReportComponent(LikertScaleEvaluationData dataSource)
        : ReportComponent<LikertScaleEvaluationData>(dataSource)
    {
        // --- Design tokenek (elrendezés és tipográfia) ---
        /// <summary>Vízszintes külső belső margó.</summary>
        private const float OuterPaddingH = 28f;
        /// <summary>Függőleges külső belső margó.</summary>
        private const float OuterPaddingV = 18f;
        /// <summary>Címsor betűméret.</summary>
        private const float TitleSize = 18f;
        /// <summary>Meta-szövegek (leírások, feliratok) betűméret.</summary>
        private const float MetaSize = 10f;
        /// <summary>Törzsszöveg betűméret.</summary>
        private const float TextSize = 11.5f;
        /// <summary>Mini sávdiagram magassága.</summary>
        private const float BarHeight = 8f;
        /// <summary>Mini sávdiagram szélessége.</summary>
        private const float BarWidth = 180f;

        // --- Szín tokenek ---
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
        /// 1) Címsor (kérdés szövege),
        /// 2) Üres állapot (ha nincs érvényes válasz),
        /// 3) Meta (mintanagyság, átlag, medián, szórás),
        /// 4) Eloszlás táblázat mini sávdiagramokkal,
        /// 5) Részletes statisztikák (min, max, módusz, indexek),
        /// 6) Értelmező megjegyzés (ha van).
        /// </para>
        /// </summary>
        /// <param name="container">A QuestPDF konténer, amelybe a komponens renderel.</param>
        public override void Compose(IContainer container)
        {
            var data = DataSource;
            var answers = data.Answers.IsDefaultOrEmpty ? ImmutableArray<int>.Empty : data.Answers;
            int n = answers.Length;

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // 1) Címsor
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // 2) Üres állapot
                    if (n == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("Ehhez a kérdéshez nem érkezett érvényes válasz.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    // 3) Meta (N, Átlag, Medián, Szórás)
                    col.Item().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                        t.Span("Válaszok száma: ");
                        t.Span(n.ToString(CultureInfo.InvariantCulture)).SemiBold();

                        t.Span("   •   Átlag: ");
                        t.Span(data.MeanValue.ToString("0.00", CultureInfo.InvariantCulture)).SemiBold();

                        t.Span("   •   Medián: ");
                        t.Span(data.MedianValue.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();

                        t.Span("   •   Szórás: ");
                        t.Span(data.StandardDeviation.ToString("0.00", CultureInfo.InvariantCulture)).SemiBold();
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // 4) Eloszlás táblázat mini sávdiagrammal
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);    // skálaérték
                            c.RelativeColumn();      // mini sáv + %
                            c.ConstantColumn(50);    // darabszám
                        });

                        // Fejléc
                        table.Cell().PaddingBottom(4).Text("Ért.").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).Text("Eloszlás").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).AlignRight().Text("Db").FontSize(MetaSize).FontColor(MetaGrey);

                        int min = data.MinimumScale;
                        int max = data.MaximumScale;

                        // Abszolút gyakoriságok
                        var freq = new int[max - min + 1];
                        foreach (var v in answers)
                            if (v >= min && v <= max) freq[v - min]++;

                        for (int i = 0; i < freq.Length; i++)
                        {
                            int value = min + i;
                            int count = freq[i];
                            double pct = n > 0 ? (double)count / n * 100.0 : 0.0;
                            float filled = (float)(BarWidth * (pct / 100.0));

                            // 4.1 Skálaérték
                            table.Cell().PaddingVertical(6)
                                .Text(value.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);

                            // 4.2 Mini sáv + százalék
                            table.Cell().PaddingVertical(6).Row(row =>
                            {
                                row.AutoItem()
                                   .Width(BarWidth)
                                   .Height(BarHeight)
                                   .Background(TrackGrey)
                                   .Border(0.5f).BorderColor(SubtleGrey)
                                   .Column(c2 =>
                                   {
                                       c2.Spacing(0);
                                       c2.Item()
                                          .Width(filled)
                                          .Height(BarHeight)
                                          .Background(AccentBlue);
                                   });

                                row.AutoItem().PaddingLeft(8)
                                   .Text(pct.ToString("0.#", CultureInfo.InvariantCulture) + "%")
                                       .FontSize(MetaSize)
                                       .FontColor(MetaGrey);
                            });

                            // 4.3 Darabszám
                            table.Cell().PaddingVertical(6).AlignRight()
                                .Text(count.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // 5) Részletes statisztikák
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        void StatCell(string label, string value)
                        {
                            t.Cell().PaddingVertical(3).Text(txt =>
                            {
                                txt.Span(label + ": ").FontSize(MetaSize).FontColor(MetaGrey);
                                txt.Span(value).FontSize(MetaSize).SemiBold().FontColor(TextBlack);
                            });
                        }

                        StatCell("Minimum", data.MinimumRate.ToString(CultureInfo.InvariantCulture));
                        StatCell("Maximum", data.MaximumRate.ToString(CultureInfo.InvariantCulture));
                        StatCell("Módusz", data.ModeValue.ToString("0.##", CultureInfo.InvariantCulture));
                        StatCell("Elégedettségi index", data.SatisfactionIndex.ToString("0.0", CultureInfo.InvariantCulture));
                        StatCell("Egyetértési arány", data.AgreementRate.ToString("0.0", CultureInfo.InvariantCulture) + "%");
                        t.Cell(); // üres helykitöltő a rugalmas 3 oszlopos elrendezéshez
                    });

                    // 6) Értelmező megjegyzés
                    if (!string.IsNullOrWhiteSpace(data.ValueMeanings))
                    {
                        col.Item().PaddingTop(6).Text(data.ValueMeanings)
                            .FontSize(MetaSize)
                            .FontColor(MetaGrey)
                            .Italic();
                    }
                });
        }
    }
}
