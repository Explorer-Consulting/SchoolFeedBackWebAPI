using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    public sealed class DropDownReportComponent(
        DropDownEvaluationData dataSource)
        : ReportComponent<DropDownEvaluationData>(dataSource)
    {
        public override void Compose(IContainer container)
        {
            var data = DataSource;

            container
                .PaddingHorizontal(28)
                .PaddingVertical(18)
                .Column(column =>
                {
                    column.Spacing(10);

                    column.Item()
                        .Text(data.QuestionStatement)
                        .FontSize(18)
                        .SemiBold();

                    column.Item()
                        .Text($"Érvényes válaszok száma: {data.ValidAnswerCount}")
                        .FontSize(10);

                    if (data.UnknownAnswerCount > 0)
                    {
                        column.Item()
                            .Text(
                                $"Ismeretlen lehetőségre hivatkozó válaszok: " +
                                $"{data.UnknownAnswerCount}")
                            .FontSize(10);
                    }

                    column.Item()
                        .LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten3);

                    if (data.ValidAnswerCount == 0)
                    {
                        column.Item()
                            .Text("Nem érkezett értelmezhető válasz ehhez a kérdéshez.");

                        return;
                    }

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(70);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Padding(6)
                                .Text("Lehetőség").SemiBold();

                            header.Cell().Padding(6).AlignRight()
                                .Text("Darab").SemiBold();

                            header.Cell().Padding(6).AlignRight()
                                .Text("Arány").SemiBold();
                        });

                        foreach (var item in data.Items)
                        {
                            int count = data.Frequencies[item.Id];
                            double percent = data.RelativeFrequencies[item.Id];

                            table.Cell().Padding(6)
                                .Text(item.Label);

                            table.Cell().Padding(6).AlignRight()
                                .Text(count.ToString());

                            table.Cell().Padding(6).AlignRight()
                                .Text($"{percent:F1}%");
                        }
                    });
                });
        }
    }
}