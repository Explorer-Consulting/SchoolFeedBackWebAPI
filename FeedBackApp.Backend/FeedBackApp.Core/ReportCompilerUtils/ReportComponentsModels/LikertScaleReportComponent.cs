using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    public sealed class LikertScaleReportComponent(LikertScaleEvaluationData dataSource)
        : ReportComponent<LikertScaleEvaluationData>(dataSource)
    {
        // design tokenek
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float MetaSize = 10f;
        private const float TextSize = 11.5f;
        private const float BarHeight = 8f;
        private const float BarWidth = 180f;

        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string TrackGrey = Colors.Grey.Lighten3;
        private static readonly string AccentBlue = Colors.Blue.Medium;
        private static readonly string PageWhite = Colors.White;

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

                    // cím
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

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

                    // meta
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

                    // eloszlás tábla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);    // érték
                            c.RelativeColumn();      // mini-sáv + %
                            c.ConstantColumn(50);    // darab
                        });

                        // fejléc
                        table.Cell().PaddingBottom(4).Text("Ért.").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).Text("Eloszlás").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).AlignRight().Text("Db").FontSize(MetaSize).FontColor(MetaGrey);

                        int min = data.MinimumScale;
                        int max = data.MaximumScale;

                        // gyakoriságok
                        var freq = new int[max - min + 1];
                        foreach (var v in answers)
                            if (v >= min && v <= max) freq[v - min]++;

                        for (int i = 0; i < freq.Length; i++)
                        {
                            int value = min + i;
                            int count = freq[i];
                            double pct = n > 0 ? (double)count / n * 100.0 : 0.0;
                            float filled = (float)(BarWidth * (pct / 100.0));

                            // 1: skálaérték
                            table.Cell().PaddingVertical(6)
                                .Text(value.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);

                            // 2: mini-sáv + százalék
                            table.Cell().PaddingVertical(6).Row(row =>
                            {
                                // track + filled
                                row.AutoItem()
                               .Width(BarWidth)
                               .Height(BarHeight)
                               .Background(TrackGrey)
                               .Border(0.5f).BorderColor(SubtleGrey)
                               .Column(col =>                 
                               {
                                   col.Spacing(0);
                                   col.Item()                 
                                      .Width(filled)
                                      .Height(BarHeight)
                                      .Background(AccentBlue);
                               });

                                row.AutoItem().PaddingLeft(8)
                                   .Text(pct.ToString("0.#", CultureInfo.InvariantCulture) + "%")
                                       .FontSize(MetaSize)
                                       .FontColor(MetaGrey);
                            });

                            // 3: darab
                            table.Cell().PaddingVertical(6).AlignRight()
                                .Text(count.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // részletes statok
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
                        t.Cell();
                    });

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
