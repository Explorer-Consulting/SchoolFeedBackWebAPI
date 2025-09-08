using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Többválasztós (Multiple Choice) kérdés kiértékelési adatmodellje.
    /// <para>
    /// Bemenetként az opciók listáját és a beérkezett válaszokat (opcióindexek) kapja,
    /// kimenetként abszolút és relatív gyakoriságokat számol, valamint (opcionálisan)
    /// együtt-előfordulásokat is tárolhat.
    /// </para>
    /// </summary>
    public sealed class MultipleChoiceEvaluationData(
        string questionStatement,
        ImmutableArray<string> answerOptions,
        ImmutableArray<int> answers
    ) : EvaluationData
    {
        #region Inputs

        /// <summary>A kérdés szövege.</summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>Válaszopciók listája (indexek ezekre hivatkoznak).</summary>
        public ImmutableArray<string> AnswerOptions { get; init; } = answerOptions;

        /// <summary>
        /// Beérkezett válaszok opcióindexei.
        /// <para>
        /// Minden elem egy kiválasztott opció indexe az <see cref="AnswerOptions"/> tömbben.
        /// </para>
        /// </summary>
        public ImmutableArray<int> Answers { get; init; } = answers;

        #endregion

        #region Outputs

        /// <summary>Abszolút gyakoriság opciónév szerint.</summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>Relatív gyakoriság százalékban opciónév szerint.</summary>
        public Dictionary<string, double> RelativeFrequenciesPercent { get; private set; } = [];

        /// <summary>
        /// Együtt-előfordulási számláló: (A,B) → hányszor szerepeltek együtt.
        /// <para>
        /// Megjegyzés: valódi együtt-előforduláshoz per-kitöltő <b>válaszhalmazokra</b> van szükség.
        /// Ha csak lapos indexlista áll rendelkezésre, ezt nem lehet helyesen számolni.
        /// </para>
        /// </summary>
        public Dictionary<(string A, string B), int> Cooccurrences { get; private set; } = [];

        #endregion

        /// <summary>
        /// Kiértékelés futtatása: abszolút és relatív gyakoriságok számítása.
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            // 1) Abszolút gyakoriság (opciónév → db)
            Frequencies = CalculateFrequency(AnswerOptions, Answers);

            // 2) Relatív gyakoriság (%) – összes db alapján
            RelativeFrequenciesPercent = CalculateRelativeFrequencyPercent(Frequencies);

            // 3) Együtt-előfordulások:
            // FIGYELEM: ehhez per-kitöltő válaszhalmazokra lenne szükség (pl. IEnumerable<int[]>).
            // Jelen bemenet (lapos indexlista) nem tartalmazza ezt az információt, ezért itt nem számolunk.
            // Cooccurrences = ...

            return this;
        }

        /// <summary>A hozzá tartozó riportkomponens (PDF) előállítása.</summary>
        public override IComponent CompileComponent()
        {
            return new MultipleChoiceReportComponent(this);
        }
    }
}
