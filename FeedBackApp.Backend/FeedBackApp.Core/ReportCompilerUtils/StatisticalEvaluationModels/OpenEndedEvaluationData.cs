using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Nyílt végű (szöveges) kérdés kiértékelési adatmodellje.
    /// <para>
    /// Tárolja a kérdés szövegét és a beérkezett szöveges válaszokat.
    /// Nem végez számszerű statisztikai kiértékelést, a <see cref="EvaluateData"/>
    /// egyszerűen az aktuális példányt adja vissza.
    /// </para>
    /// </summary>
    public sealed class OpenEndedEvaluationData(string questionStatement, ImmutableArray<string> answers)
        : EvaluationData
    {
        /// <summary>
        /// A kérdés szövege.
        /// </summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>
        /// A beérkezett szöveges válaszok listája.
        /// <para>
        /// Üres vagy whitespace-only stringek szűrése ajánlott már a betöltésnél.
        /// </para>
        /// </summary>
        public ImmutableArray<string> Answers { get; init; } = answers;

        /// <summary>
        /// Nem számol további mutatókat, egyszerűen az aktuális példányt adja vissza.
        /// </summary>
        public override EvaluationData EvaluateData() => this;

        /// <summary>
        /// A nyílt végű kérdéshez tartozó riportkomponens létrehozása.
        /// </summary>
        public override IComponent CompileComponent() => new OpenEndedReportComponent(this);
    }
}
