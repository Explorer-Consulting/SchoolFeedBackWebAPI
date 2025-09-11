using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Report component for open-ended (text) questions.
    /// <para>
    /// Displays the question text, the number of received text responses,
    /// a warning frame in case of no responses, otherwise shows the answers
    /// in quotation marks inside separate cards. At the end, it displays
    /// an anonymity note.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="number">
    /// <item>Instantiate the component with <see cref="OpenEndedEvaluationData"/>.</item>
    /// <item>Add it to the document’s <c>ReportComponents</c> list.</item>
    /// <item>During rendering, the component displays the text answers card by card.</item>
    /// </list>
    /// Preconditions:
    /// <list type="bullet">
    /// <item><see cref="OpenEndedEvaluationData.QuestionStatement"/> – the question text.</item>
    /// <item><see cref="OpenEndedEvaluationData.Answers"/> – the list of responses (empty items should be filtered in advance).</item>
    /// </list>
    /// </remarks>
    public sealed class OpenEndedReportComponent(OpenEndedEvaluationData dataSource)
         : ReportComponent<OpenEndedEvaluationData>(dataSource)
    {
        // --- Dimensions ---
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float AnswerSize = 11.5f;
        private const float MetaSize = 10f;

        // --- Color tokens ---
        private static readonly string FrameBlue = Colors.Blue.Medium;
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string PageWhite = Colors.White;

        /// <summary>
        /// Describes how the component is rendered.
        /// <para>
        /// Sections:
        /// 1) Title (question),  
        /// 2) Meta (number of answers),  
        /// 3) Separator line,  
        /// 4) Empty state with message (if no answers),  
        /// 5) Answers displayed in cards (quoted),  
        /// 6) Anonymity note.  
        /// </para>
        /// </summary>
        /// <param name="container">The QuestPDF container into which the component renders.</param>
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

                    // 1) Title
                    col.Item().Text(DataSource.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // 2) Meta
                    col.Item().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                        t.Span("Válaszok száma: ");
                        t.Span(answers.Length.ToString()).SemiBold();
                    });

                    // 3) Separator
                    col.Item()
                        .LineHorizontal(1)
                        .LineColor(SubtleGrey);

                    // 4) Empty state
                    if (answers.Length == 0)
                    {
                        col.Item()
                            .Background(PageWhite)
                            .Border(1).BorderColor(FrameBlue)
                            .Padding(12)
                            .Text("Nem érkezett értelmezhető válasz ehhez a kérdéshez.")
                                .FontSize(AnswerSize).FontColor(MetaGrey);
                        return;
                    }

                    // 5) Answers in cards
                    foreach (var a in answers)
                    {
                        col.Item()
                            .Background(PageWhite)
                            .Border(1).BorderColor(FrameBlue)
                            .Padding(12)
                            .Text($"“{a}”")
                                .FontSize(AnswerSize)
                                .FontColor(TextBlack)
                                .Italic()
                                .LineHeight(1.32f);
                    }

                    // 6) Anonymity note
                    col.Item().Text("A szöveges válaszok anonim módon kerültek feldolgozásra.")
                        .FontSize(9).FontColor(MetaGrey).Italic();
                });
        }
    }
}
