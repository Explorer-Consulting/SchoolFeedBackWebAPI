using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    public sealed class OpenEndedReportComponent(OpenEndedEvaluationData dataSource)
         : ReportComponent<OpenEndedEvaluationData>(dataSource)
    {
        // meretek
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float AnswerSize = 11.5f;
        private const float MetaSize = 10f;

        // szintokenek
        private static readonly string FrameBlue = Colors.Blue.Medium;
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string PageWhite = Colors.White;

        public override void Compose(IContainer container)
        {
            var answers = DataSource.Answers.IsDefaultOrEmpty
                ? []
                : DataSource.Answers;

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // Cim
                    col.Item().Text(DataSource.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // Meta
                    col.Item().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                        t.Span("Válaszok száma: ");
                        t.Span(answers.Length.ToString()).SemiBold();
                    });

                    // Elvalaszto
                    col.Item()
                        .LineHorizontal(1)
                        .LineColor(SubtleGrey);

                    // ures allapot
                    if (answers.Length == 0)
                    {
                        col.Item()
                            .Background(PageWhite)
                            .Border(1).BorderColor(FrameBlue)
                            .Padding(12)
                            .Text("Ehhez a kérdéshez nem érkezett szöveges válasz.")
                                .FontSize(AnswerSize).FontColor(MetaGrey);
                        return;
                    }

                    foreach (var a in answers)
                    {
                        col.Item()
                            .Background(PageWhite)
                            .Border(1).BorderColor(FrameBlue)
                            .Padding(12)
                            .Text($"„{a}”")
                                .FontSize(AnswerSize)
                                .FontColor(TextBlack)
                                .Italic()
                                .LineHeight(1.32f);
                    }

                    col.Item().Text("A szöveges válaszok anonim módon kerültek feldolgozásra.")
                        .FontSize(9).FontColor(MetaGrey).Italic();
                });
        }
    }
}
