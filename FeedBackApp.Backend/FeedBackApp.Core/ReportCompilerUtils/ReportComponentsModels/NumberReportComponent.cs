using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    public sealed class NumberReportComponent(NumberEvaluationData dataSource)
        : ReportComponent<NumberEvaluationData>(dataSource)
    {
        public override void Compose(IContainer container)
        {
            var d = DataSource;
            var count = d.Answers.IsDefaultOrEmpty ? 0 : d.Answers.Length;

            container.PaddingHorizontal(28).PaddingVertical(18).Column(col =>
            {
                col.Spacing(10);

                col.Item().Text(d.QuestionStatement).FontSize(18).SemiBold().FontColor(Colors.Grey.Darken4);

                col.Item().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken2));
                    t.Span("Válaszok száma: ");
                    t.Span(count.ToString()).SemiBold();
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                if (count == 0)
                {
                    col.Item().Border(1).BorderColor(Colors.Blue.Medium).Padding(12)
                        .Text("Nem érkezett értelmezhető válasz ehhez a kérdéshez.")
                        .FontSize(11.5f).FontColor(Colors.Grey.Darken2);
                    return;
                }

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                    void StatRow(string label, double value)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).Text(label);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6).AlignRight()
                            .Text(value.ToString("0.##", CultureInfo.InvariantCulture)).SemiBold();
                    }

                    StatRow("Átlag", d.Mean);
                    StatRow("Medián", d.Median);
                    StatRow("Minimum", d.Minimum);
                    StatRow("Maximum", d.Maximum);
                    StatRow("Szórás", d.StandardDeviation);
                });
            });
        }
    }
}