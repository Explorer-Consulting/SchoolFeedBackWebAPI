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
    /// Két megjelenítési ágat támogat:
    /// <list type="bullet">
    /// <item><see cref="SingleChoice.REGULAR"/>: opciók eloszlása mini sávdiagrammal (% és db), statisztikákkal (N, átlag, medián, módusz).</item>
    /// <item><c>OTHER</c> (szöveges „Egyéb” válaszok): a beírt szöveges válaszok listázása kártyákban.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>
    /// Elvárt adatok:
    /// <list type="bullet">
    /// <item><see cref="SingleChoiceEvaluationData.QuestionStatement"/> – a kérdés szövege.</item>
    /// <item><see cref="SingleChoiceEvaluationData.QuestionOptions"/> – az opciók (eredeti sorrendben).</item>
    /// <item><see cref="SingleChoiceEvaluationData.QuestionOptionAnswers"/> – a válaszok indexei (REGULAR eset).</item>
    /// <item><see cref="SingleChoiceEvaluationData.Frequencies"/> és <see cref="SingleChoiceEvaluationData.RelativeFrequencies"/> – eloszlási mutatók.</item>
    /// <item><see cref="SingleChoiceEvaluationData.QuestionOpenAnswers"/> – szabad szöveges „Egyéb” válaszok (OTHER eset).</item>
    /// </list>
    /// Használat:
    /// <list type="number">
    /// <item>Példányosítsd a komponenst egy <see cref="SingleChoiceEvaluationData"/> adattal.</item>
    /// <item>Add a dokumentum <c>ReportComponents</c> listájához.</item>
    /// <item>Rendereléskor a komponens a típusnak megfelelő elrendezést jeleníti meg.</item>
    /// </list>
    /// </remarks>
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

        /// <summary>
        /// A komponens megjelenítésének leírása.
        /// <para>
        /// Ágak:
        /// <list type="number">
        /// <item><b>REGULAR</b>: üres állapot ellenőrzések → meta (N, átlag, medián, módusz) → eloszlás táblázat
        /// (opció szöveg, mini sáv + %, darab), az opciók eredeti sorrendjét megtartva.</item>
        /// <item><b>OTHER</b>: üres állapot ellenőrzés → válaszok egyszerű dobozokban (szöveges lista).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="container">A QuestPDF konténer, amelybe a komponens renderel.</param>
        public override void Compose(IContainer container)
        {
            var data = DataSource;

            var options = data.QuestionOptions.IsDefaultOrEmpty ? [] : data.QuestionOptions;
            var answersIdx = data.QuestionOptionAnswers.IsDefaultOrEmpty ? [] : data.QuestionOptionAnswers;
            var openAnswers = data.QuestionOpenAnswers.IsDefaultOrEmpty
                ? []
                : data.QuestionOpenAnswers.Where(s => !string.IsNullOrWhiteSpace(s)).ToImmutableArray();

            int n = answersIdx.Length;

            container
                .PaddingHorizontal(OuterPaddingH)
                .PaddingVertical(OuterPaddingV)
                .Column(col =>
                {
                    col.Spacing(10);

                    // Cím
                    col.Item().Text(data.QuestionStatement)
                        .FontSize(TitleSize).SemiBold()
                        .FontColor(TextBlack)
                        .LineHeight(1.35f);

                    // --- REGULAR ág: diszkrét opciók százalékkal és darabszámmal ---
                    if (data.Type is SingleChoice.REGULAR)
                    {
                        // Üres állapotok
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

                        // Meta – N, alapsztat
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

                        // Eloszlás tábla
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

                            // Megjelenítés az opciók eredeti sorrendjében
                            foreach (var option in options)
                            {
                                freq.TryGetValue(option, out var count);
                                double pct = 0.0;

                                if (rel.TryGetValue(option, out var rpct))
                                    pct = rpct;
                                else if (n > 0)
                                    pct = (double)count / n * 100.0;

                                float filled = (float)(BarWidth * (pct / 100.0));

                                // 1) Opció
                                table.Cell().PaddingVertical(6)
                                    .Text(option)
                                    .FontSize(TextSize).FontColor(TextBlack);

                                // 2) Mini sáv + %
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

                                // 3) Darab
                                table.Cell().PaddingVertical(6).AlignRight()
                                    .Text(count.ToString(CultureInfo.InvariantCulture))
                                    .FontSize(TextSize).FontColor(TextBlack);
                            }
                        });
                    }
                    // --- OTHER ág: szabad szöveges „Egyéb” válaszok ---
                    else
                    {
                        if (openAnswers.Length == 0)
                        {
                            col.Item()
                               .Background(PageWhite)
                               .Border(1).BorderColor(AccentBlue)
                               .Padding(12)
                               .Text("Ehhez a kérdéshez nem érkezett szöveges válasz.")
                                   .FontSize(TextSize).FontColor(MetaGrey);
                            return;
                        }

                        // Meta – N
                        col.Item().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(MetaSize).FontColor(MetaGrey));
                            t.Span("Válaszok száma: ");
                            t.Span(openAnswers.Length.ToString(CultureInfo.InvariantCulture)).SemiBold();
                        });

                        col.Item().LineHorizontal(1).LineColor(SubtleGrey);

                        // Felsorolás dobozokban
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
                });
        }
    }
}
