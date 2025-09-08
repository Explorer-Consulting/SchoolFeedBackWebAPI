using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels
{
    /// <summary>
    /// Nyílt végű (szöveges) kérdésekhez tartozó riportkomponens.
    /// <para>
    /// Megjeleníti a kérdés szövegét, a beérkezett szöveges válaszok számát,
    /// üres állapotban figyelmeztető keretet, egyébként pedig a válaszokat
    /// idézőjellel, elkülönített kártyákban. A végén anonimitási megjegyzést jelenít meg.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Használat:
    /// <list type="number">
    /// <item>Példányosítsd a komponenst <see cref="OpenEndedEvaluationData"/> adattal.</item>
    /// <item>Add a dokumentum <c>ReportComponents</c> listájához.</item>
    /// <item>Rendereléskor a komponens a szöveges válaszokat kártyánként jeleníti meg.</item>
    /// </list>
    /// Előfeltételek:
    /// <list type="bullet">
    /// <item><see cref="OpenEndedEvaluationData.QuestionStatement"/> – a kérdés szövege.</item>
    /// <item><see cref="OpenEndedEvaluationData.Answers"/> – a válaszok listája (üres elemeket érdemes előzetesen szűrni).</item>
    /// </list>
    /// </remarks>
    public sealed class OpenEndedReportComponent(OpenEndedEvaluationData dataSource)
         : ReportComponent<OpenEndedEvaluationData>(dataSource)
    {
        // --- Méretek ---
        private const float OuterPaddingH = 28f;
        private const float OuterPaddingV = 18f;
        private const float TitleSize = 18f;
        private const float AnswerSize = 11.5f;
        private const float MetaSize = 10f;

        // --- Színtokenek ---
        private static readonly string FrameBlue = Colors.Blue.Medium;
        private static readonly string TextBlack = Colors.Grey.Darken4;
        private static readonly string MetaGrey = Colors.Grey.Darken2;
        private static readonly string SubtleGrey = Colors.Grey.Lighten3;
        private static readonly string PageWhite = Colors.White;

        /// <summary>
        /// A komponens megjelenítésének leírása.
        /// <para>
        /// Szekciók:
        /// 1) Címsor (kérdés),
        /// 2) Meta (válaszok száma),
        /// 3) Elválasztó vonal,
        /// 4) Üres állapot üzenettel (ha nincs válasz),
        /// 5) Válaszok kártyákban (idézve),
        /// 6) Anonimitási megjegyzés.
        /// </para>
        /// </summary>
        /// <param name="container">A QuestPDF konténer, amelybe a komponens renderel.</param>
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

                    // 1) Címsor
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

                    // 3) Elválasztó
                    col.Item()
                        .LineHorizontal(1)
                        .LineColor(SubtleGrey);

                    // 4) Üres állapot
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

                    // 5) Válaszok kártyákban
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

                    // 6) Anonimitási megjegyzés
                    col.Item().Text("A szöveges válaszok anonim módon kerültek feldolgozásra.")
                        .FontSize(9).FontColor(MetaGrey).Italic();
                });
        }
    }
}
