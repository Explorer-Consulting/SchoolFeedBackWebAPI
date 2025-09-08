using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Egyválasztós (Single Choice) kérdés kiértékelési adatmodellje.
    /// <para>
    /// Kétféle kérdéstípus támogatott:
    /// <list type="bullet">
    /// <item><see cref="SingleChoice.REGULAR"/> – előre definiált opciók közül választás (számított statisztikákkal).</item>
    /// <item><see cref="SingleChoice.CUSTOM"/> – szabad szöveges válaszok („Egyéb” opciók) statisztika nélkül.</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class SingleChoiceEvaluationData(
        string questionStatement,
        ImmutableArray<string> questionOptions,
        SingleChoice type,
        ImmutableArray<int> questionOptionAnswers,
        ImmutableArray<string> questionOpenAnswers
    ) : EvaluationData
    {
        #region Question-specific properties

        /// <summary>A kérdés szövege.</summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>Előre definiált opciók listája.</summary>
        public ImmutableArray<string> QuestionOptions = questionOptions;

        /// <summary>A kitöltők által megadott válaszok indexei az <see cref="QuestionOptions"/> tömbben.</summary>
        public ImmutableArray<int> QuestionOptionAnswers = questionOptionAnswers;

        /// <summary>Szabad szöveges válaszok listája („Egyéb” opciók).</summary>
        public ImmutableArray<string> QuestionOpenAnswers = questionOpenAnswers;

        /// <summary>A kérdés típusa: <see cref="SingleChoice.REGULAR"/> vagy <see cref="SingleChoice.CUSTOM"/>.</summary>
        public SingleChoice Type = type;

        /// <summary>Átlag (REGULAR esetben).</summary>
        public double MeanValue { get; private set; }

        /// <summary>Medián (REGULAR esetben).</summary>
        public double MedianValue { get; private set; }

        /// <summary>Módusz (REGULAR esetben).</summary>
        public double ModeValue { get; private set; }

        /// <summary>Abszolút gyakoriság (REGULAR esetben).</summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>Relatív gyakoriság % (REGULAR esetben).</summary>
        public Dictionary<string, double> RelativeFrequencies { get; private set; } = [];

        #endregion

        /// <summary>
        /// Kiértékelés futtatása.
        /// <para>
        /// Ha a típus <see cref="SingleChoice.REGULAR"/>, akkor számítja az átlagot,
        /// mediánt, móduszt, valamint az abszolút és relatív gyakoriságokat.
        /// Ha <see cref="SingleChoice.CUSTOM"/>, akkor nem számol statisztikát,
        /// csak a szöveges válaszok érhetők el.
        /// </para>
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            if (Type != SingleChoice.REGULAR)
            {
                return this;
            }

            MeanValue = CalculateMeanValue(QuestionOptionAnswers);
            MedianValue = CalculateMedianValue(QuestionOptionAnswers);
            ModeValue = CalculateModeValue(QuestionOptionAnswers);
            Frequencies = CalculateFrequency(QuestionOptions, QuestionOptionAnswers);
            RelativeFrequencies = CalculateRelativeFrequencyPercent(Frequencies, QuestionOptionAnswers.Length);

            return this;
        }

        /// <summary>
        /// A hozzá tartozó riportkomponens (PDF) előállítása.
        /// </summary>
        public override IComponent CompileComponent()
        {
            return new SingleChoiceReportComponent(this);
        }
    }
}
