using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Report component for Likert-scale questions.
    /// <para>
    /// Displays the question text, sample size, basic descriptive statistics
    /// (mean, median, standard deviation), the distribution of values
    /// with a mini bar chart and table, as well as detailed indicators
    /// (minimum, maximum, mode, satisfaction index, agreement rate).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Instantiate the component with a <see cref="LikertScaleEvaluationData"/> instance.</item>
    /// <item>Add the component to a document’s <c>ReportComponents</c> list.</item>
    /// <item>During document generation, the component will render into the corresponding section of the content.</item>
    /// </list>
    /// </remarks>
    public sealed class LikertScaleReportComponent(LikertScaleEvaluationData dataSource)
        : ReportComponent<LikertScaleEvaluationData>(dataSource)
    {
        // --- Design tokens (layout and typography) ---
        /// <summary>Horizontal outer padding.</summary>
        private const float OuterPaddingH = 28f;
        /// <summary>Vertical outer padding.</summary>
        private const float OuterPaddingV = 18f;
        /// <summary>Title font size.</summary>
        private const float TitleSize = 18f;
        /// <summary>Font size for meta texts (descriptions, labels).</summary>
        private const float MetaSize = 10f;
        /// <summary>Body text font size.</summary>
        private const float TextSize = 11.5f;
        /// <summary>Height of the mini bar chart.</summary>
        private const float BarHeight = 8f;
        /// <summary>Width of the mini bar chart.</summary>
        private const float BarWidth = 180f;

        // --- Color tokens ---
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string TrackGrey = Colors.Grey.Lighten3;
        private static readonly string AccentBlue = Colors.Blue.Medium;
        private static readonly string PageWhite = Colors.White;

        /// <summary>
        /// Describes the rendering of the component.
        /// <para>
        /// Sections:
        /// 1) Title (question text),  
        /// 2) Empty state (if no valid answers),  
        /// 3) Meta (sample size, mean, median, standard deviation),  
        /// 4) Distribution table with mini bar charts,  
        /// 5) Detailed statistics (min, max, mode, indexes),  
        /// 6) Interpretive note (if present).  
        /// </para>
        /// </summary>
        /// <param name="container">QuestPDF container into which the component is rendered.</param>
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

                    // 1) Title
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // 2) Empty state
                    if (n == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("Nem érkezett értelmezhető válasz ehhez a kérdéshez.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    // 3) Meta (N, Mean, Median, Standard Deviation)
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

                    // 4) Distribution table with mini bar chart
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);    // scale value
                            c.RelativeColumn();      // mini bar + %
                            c.ConstantColumn(50);    // count
                        });

                        // Header
                        table.Cell().PaddingBottom(4).Text("Érték").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).Text("Eloszlás").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).AlignRight().Text("N").FontSize(MetaSize).FontColor(MetaGrey);

                        int min = data.MinimumScale;
                        int max = data.MaximumScale;

                        // Absolute frequencies
                        var freq = new int[max - min + 1];
                        foreach (var v in answers)
                            if (v >= min && v <= max) freq[v - min]++;

                        for (int i = 0; i < freq.Length; i++)
                        {
                            int value = min + i;
                            int count = freq[i];
                            double pct = n > 0 ? (double)count / n * 100.0 : 0.0;
                            float filled = (float)(BarWidth * (pct / 100.0));

                            // 4.1 Scale value
                            table.Cell().PaddingVertical(6)
                                .Text(value.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);

                            // 4.2 Mini bar + percentage
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

                            // 4.3 Count
                            table.Cell().PaddingVertical(6).AlignRight()
                                .Text(count.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // 5) Detailed statistics
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
                        StatCell("Elégedettségi Index", data.SatisfactionIndex.ToString("0.0", CultureInfo.InvariantCulture));
                        StatCell("Egyetértési Index", data.AgreementRate.ToString("0.0", CultureInfo.InvariantCulture) + "%");
                        t.Cell(); // empty filler for flexible 3-column layout
                    });

                    // 6) Interpretive note
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
