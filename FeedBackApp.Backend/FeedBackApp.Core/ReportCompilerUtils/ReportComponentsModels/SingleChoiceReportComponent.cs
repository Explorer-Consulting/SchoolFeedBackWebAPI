using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Egyválasztós (Single Choice) kérdések riportkomponense.
    /// <para>
    /// Típusok:
    /// <list type="bullet">
    /// <item><see cref="SingleChoice.REGULAR"/>: opciók eloszlása mini sávdiagrammal (% és db), statisztikákkal (N, átlag, medián, módusz).</item>
    /// <item><see cref="SingleChoice.CUSTOM"/>: numerikus értékek statisztikái + gyakoriság, szöveges „Egyéb” válaszok külön listában.</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class SingleChoiceReportComponent(SingleChoiceEvaluationData dataSource)
        : ReportComponent<SingleChoiceEvaluationData>(dataSource)
    {
        // --- Design tokenek (elrendezés és tipográfia) ---
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float MetaSize = 10f;
        private const float TextSize = 11.5f;
        private const float BarHeight = 8f;
        private const float BarWidth = 180f;

        // --- Szín tokenek ---
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string TrackGrey = Colors.Grey.Lighten3;
        private static readonly string AccentBlue = Colors.Blue.Medium;
        private static readonly string PageWhite = Colors.White;

        public override void Compose(IContainer container)
        {
            var data = DataSource;

            var options = data.QuestionOptions.IsDefaultOrEmpty ? [] : data.QuestionOptions;
            var answersIdx = data.QuestionOptionAnswers.IsDefaultOrEmpty ? [] : data.QuestionOptionAnswers;
            var openAnswers = data.QuestionOpenAnswers.IsDefaultOrEmpty
                ? []
                : data.QuestionOpenAnswers.Where(s => !string.IsNullOrWhiteSpace(s)).ToImmutableArray();

            var hasNums = answersIdx.Length > 0;
            var hasTexts = openAnswers.Length > 0;

            int n = answersIdx.Length;

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // --- Cím ---
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // --- REGULAR ág ---
                    if (data.Type is SingleChoice.REGULAR)
                    {
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

                        // Meta – N, statisztikák
                        col.Item().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                            t.Span("Válaszok száma: ");
                            t.Span(n.ToString(CultureInfo.InvariantCulture)).SemiBold();

                            t.Span("   •   Átlag: ");
                            t.Span(data.MeanValue.ToString("0.00", CultureInfo.InvariantCulture)).SemiBold();

                            t.Span("   •   Medián: ");
                            t.Span(data.MedianValue.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();

                            t.Span("   •   Módusz: ");
                            t.Span(data.ModeValue.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();
                        });

                        col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                        // Eloszlás táblázat
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

                            var freq = data.Frequencies ?? [];
                            var rel = data.RelativeFrequencies ?? [];

                            foreach (var option in options)
                            {
                                freq.TryGetValue(option, out var count);
                                double pct = 0.0;

                                if (rel.TryGetValue(option, out var rpct))
                                    pct = rpct;
                                else if (n > 0)
                                    pct = (double)count / n * 100.0;

                                float filled = (float)(BarWidth * (pct / 100.0));

                                table.Cell().PaddingVertical(6)
                                    .Text(option)
                                    .FontSize(TextSize).FontColor(TextBlack);

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

                                table.Cell().PaddingVertical(6).AlignRight()
                                    .Text(count.ToString(CultureInfo.InvariantCulture))
                                    .FontSize(TextSize).FontColor(TextBlack);
                            }
                        });
                    }
                    // --- CUSTOM ág ---
                    else
                    {
                        if (!hasNums && !hasTexts)
                        {
                            col.Item()
                               .Background(PageWhite)
                               .Border(1).BorderColor(AccentBlue)
                               .Padding(12)
                               .Text("Ehhez a kérdéshez nem érkezett válasz.")
                                   .FontSize(TextSize).FontColor(MetaGrey);
                            return;
                        }

                        if (hasNums)
                        {
                            // Meta – numerikus statisztikák
                            col.Item().Text(t =>
                            {
                                t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                                t.Span("Numerikus válaszok száma: ");
                                t.Span(n.ToString(CultureInfo.InvariantCulture)).SemiBold();

                                t.Span("   •   Átlag: ");
                                t.Span(data.MeanValue.ToString("0.00", CultureInfo.InvariantCulture)).SemiBold();

                                t.Span("   •   Medián: ");
                                t.Span(data.MedianValue.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();

                                t.Span("   •   Módusz: ");
                                t.Span(data.ModeValue.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();
                            });

                            col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                            // Gyakoriság táblázat (kulcsok: numerikus értékek szövegesen)
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);     // érték
                                    c.RelativeColumn(2);     // mini-sáv + %
                                    c.ConstantColumn(50);    // darab
                                });

                                // Fejléc
                                table.Cell().PaddingBottom(4).Text("Érték").FontSize(MetaSize).FontColor(MetaGrey);
                                table.Cell().PaddingBottom(4).Text("Eloszlás").FontSize(MetaSize).FontColor(MetaGrey);
                                table.Cell().PaddingBottom(4).AlignRight().Text("Db").FontSize(MetaSize).FontColor(MetaGrey);

                                var freq = data.Frequencies ?? [];
                                var rel = data.RelativeFrequencies ?? [];

                                foreach (var (key, count) in freq.OrderByDescending(kv => kv.Value))
                                {
                                    double pct = 0.0;
                                    if (rel.TryGetValue(key, out var rpct))
                                        pct = rpct;
                                    else if (n > 0)
                                        pct = (double)count / n * 100.0;

                                    float filled = (float)(BarWidth * (pct / 100.0));

                                    table.Cell().PaddingVertical(6)
                                        .Text(key)
                                        .FontSize(TextSize).FontColor(TextBlack);

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

                                    table.Cell().PaddingVertical(6).AlignRight()
                                        .Text(count.ToString(CultureInfo.InvariantCulture))
                                        .FontSize(TextSize).FontColor(TextBlack);
                                }
                            });
                        }

                        if (hasTexts)
                        {
                            col.Item().LineHorizontal(1).LineColor(SubtleGrey);
                            col.Item().Text("Szöveges válaszok").FontSize(MetaSize).FontColor(MetaGrey);

                            foreach (var ans in openAnswers)
                            {
                                col.Item()
                                   .Background(PageWhite)
                                   .Border(0.8f).BorderColor(SubtleGrey)
                                   .Padding(8)
                                   .Text(ans)
                                       .FontSize(TextSize)
                                       .FontColor(TextBlack);
                            }
                        }
                    }
                });
        }
    }
}
