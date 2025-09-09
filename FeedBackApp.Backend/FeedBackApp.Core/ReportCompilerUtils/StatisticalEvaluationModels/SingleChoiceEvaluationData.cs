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
    /// <item><see cref="SingleChoice.CUSTOM"/> – vegyes: a számként értelmezhető válaszokból statisztikák készülnek,
    /// a nem-szám válaszok külön listában jelennek meg (statisztika nélkül).</item>
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

        /// <summary>
        /// Válaszok számosított reprezentációja.
        /// REGULAR esetben: az <see cref="QuestionOptions"/> indexei.
        /// CUSTOM esetben: a beérkezett numerikus értékek (nem indexek!), pl. 1..5.
        /// </summary>
        public ImmutableArray<int> QuestionOptionAnswers = questionOptionAnswers;

        /// <summary>Nem-szám (szabad szöveges) válaszok listája.</summary>
        public ImmutableArray<string> QuestionOpenAnswers = questionOpenAnswers;

        /// <summary>A kérdés típusa: <see cref="SingleChoice.REGULAR"/> vagy <see cref="SingleChoice.CUSTOM"/>.</summary>
        public SingleChoice Type = type;

        /// <summary>Átlag (REGULAR és CUSTOM numerikus válaszok esetén).</summary>
        public double MeanValue { get; private set; }

        /// <summary>Medián (REGULAR és CUSTOM numerikus válaszok esetén).</summary>
        public double MedianValue { get; private set; }

        /// <summary>Módusz (REGULAR és CUSTOM numerikus válaszok esetén).</summary>
        public double ModeValue { get; private set; }

        /// <summary>Abszolút gyakoriság.
        /// REGULAR: opció-nevekre aggregálva.
        /// CUSTOM: konkrét numerikus értékekre (pl. "1","2","3"...).</summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>Relatív gyakoriság % (REGULAR és CUSTOM numerikus válaszokra).</summary>
        public Dictionary<string, double> RelativeFrequencies { get; private set; } = [];

        #endregion

        /// <summary>
        /// Kiértékelés futtatása.
        /// <para>
        /// REGULAR: átlag/medián/módusz + gyakoriságok az opciók szerint.
        /// CUSTOM: ha vannak numerikus válaszok, akkor ezekből átlag/medián/módusz + gyakoriságok készülnek,
        /// a nem-szám válaszok pedig változatlanul megjelennek.
        /// </para>
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            if (Type == SingleChoice.REGULAR)
            {
                if (QuestionOptionAnswers.Length == 0)
                    return this;

                MeanValue = CalculateMeanValue(QuestionOptionAnswers);
                MedianValue = CalculateMedianValue(QuestionOptionAnswers);
                ModeValue = CalculateModeValue(QuestionOptionAnswers);
                Frequencies = CalculateFrequency(QuestionOptions, QuestionOptionAnswers);
                RelativeFrequencies = CalculateRelativeFrequencyPercent(Frequencies, QuestionOptionAnswers.Length);
                return this;
            }

            // CUSTOM
            if (QuestionOptionAnswers.Length > 0)
            {
                // Itt a QuestionOptionAnswers NEM opcióindex, hanem maga a numerikus érték (pl. 1..5)
                MeanValue = CalculateMeanValue(QuestionOptionAnswers);
                MedianValue = CalculateMedianValue(QuestionOptionAnswers);
                ModeValue = CalculateModeValue(QuestionOptionAnswers);
                Frequencies = CalculateFrequency(QuestionOptions, QuestionOptionAnswers);
                RelativeFrequencies = CalculateRelativeFrequencyPercent(Frequencies, QuestionOptionAnswers.Length);
            }
            // Ha nincsenek numerikus válaszok, akkor a stat mezők 0/üres maradnak, 
            // és csak a QuestionOpenAnswers lesz releváns a riportban.

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
