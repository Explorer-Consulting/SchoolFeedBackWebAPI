using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Report component for Multiple Choice questions.
    /// <para>
    /// Displays the question text, total number of responses, option distribution
    /// with a mini bar chart and table, and—if available—the most frequent
    /// co-occurrences (option pairs).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Instantiate the component with <see cref="MultipleChoiceEvaluationData"/>.</item>
    /// <item>Add it to the document’s <c>ReportComponents</c> list.</item>
    /// <item>During document generation, the component renders into the appropriate content section.</item>
    /// </list>
    /// Preconditions:
    /// <list type="bullet">
    /// <item><see cref="MultipleChoiceEvaluationData.AnswerOptions"/> contains the list of options (text).</item>
    /// <item><see cref="MultipleChoiceEvaluationData.Frequencies"/> contains absolute counts (option → count).</item>
    /// <item><see cref="MultipleChoiceEvaluationData.RelativeFrequenciesPercent"/> optional relative percentages.</item>
    /// </list>
    /// </remarks>
    public sealed class MultipleChoiceReportComponent(MultipleChoiceEvaluationData dataSource)
        : ReportComponent<MultipleChoiceEvaluationData>(dataSource)
    {
        // --- Design tokens ---
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float MetaSize = 10f;
        private const float TextSize = 11.5f;
        private const float BarHeight = 8f;
        private const float BarWidth = 180f;

        // --- Colors ---
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string TrackGrey = Colors.Grey.Lighten3;
        private static readonly string AccentBlue = Colors.Blue.Medium;
        private static readonly string PageWhite = Colors.White;

        /// <summary>
        /// Describes how the component is rendered.
        /// <para>
        /// Sections:
        /// 1) Title (question),  
        /// 2) Empty state (missing options or responses),  
        /// 3) Meta (total responses),  
        /// 4) Distribution table with mini bar charts,  
        /// 5) Optional: top co-occurrences (A–B pairs).  
        /// </para>
        /// </summary>
        /// <param name="container">The QuestPDF container into which the component renders.</param>
        public override void Compose(IContainer container)
        {
            var data = DataSource;

            // Safely handle inputs
            var options = data.AnswerOptions.IsDefaultOrEmpty ? [] : data.AnswerOptions;
            var answers = data.Answers.IsDefaultOrEmpty ? [] : data.Answers;

            int n = answers.Length; // total votes (records)

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // Title
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // Empty states
                    if (options.Length == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("No options are defined for this question.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    if (n == 0 || data.Frequencies is null || data.Frequencies.Count == 0)
                    {
                        col.Item()
                           .Background(PageWhite)
                           .Border(1).BorderColor(AccentBlue)
                           .Padding(12)
                           .Text("No valid responses were received for this question.")
                               .FontSize(TextSize).FontColor(MetaGrey);
                        return;
                    }

                    // Meta – total responses
                    col.Item().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                        t.Span("Number of responses: ");
                        t.Span(n.ToString(CultureInfo.InvariantCulture)).SemiBold();
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // Distribution table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);     // option text
                            c.RelativeColumn(4);     // mini bar + %
                            c.ConstantColumn(50);    // count
                        });

                        // Header
                        table.Cell().PaddingBottom(4).Text("Option").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).Text("Distribution").FontSize(MetaSize).FontColor(MetaGrey);
                        table.Cell().PaddingBottom(4).AlignRight().Text("N").FontSize(MetaSize).FontColor(MetaGrey);

                        // Rows — sorted by decreasing frequency
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

                            // 1) Option text
                            table.Cell().PaddingVertical(6)
                                .Text(option)
                                .FontSize(TextSize).FontColor(TextBlack);

                            // 2) Mini bar + percentage
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

                            // 3) Count
                            table.Cell().PaddingVertical(6).AlignRight()
                                .Text(count.ToString(CultureInfo.InvariantCulture))
                                .FontSize(TextSize).FontColor(TextBlack);
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                    // Top co-occurrences (optional)
                    if (data.Cooccurrences is not null && data.Cooccurrences.Count > 0)
                    {
                        var topPairs = data.Cooccurrences
                            .OrderByDescending(kv => kv.Value)
                            .Take(5)
                            .ToList();

                        col.Item().Text("Most frequent co-occurrences")
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
                            t.Cell().PaddingBottom(4).AlignRight().Text("N").FontSize(MetaSize).FontColor(MetaGrey);

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
