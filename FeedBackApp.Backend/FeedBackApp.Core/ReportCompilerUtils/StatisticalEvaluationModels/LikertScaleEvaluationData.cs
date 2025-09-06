using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Likert-skálás kérdéshez tartozó statisztikai kiértékelési adatmodell.
    /// <para>
    /// Tartalmazza a nyers válaszokat, a skála paramétereit, valamint a
    /// kiértékelt mutatókat (átlag, medián, módusz, szórás, min/max, elégedettségi index, egyetértési arány).
    /// </para>
    /// A <see cref="EvaluateData"/> hívása után minden számított érték feltöltésre kerül.
    /// A <see cref="CompileComponent"/> metódus a hozzá tartozó QuestPDF komponenssel tér vissza.
    /// </summary>
    public sealed class LikertScaleEvaluationData(
        string questionStatement,
        ImmutableArray<int> answers,
        string valueMeanings,
        int minimumScale,
        int maximumScale
    ) : EvaluationData
    {
        #region Question-specific properties

        /// <summary>
        /// A kérdés szövege (pl. „A tanár érthetően magyarázott”).
        /// </summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>
        /// A beérkezett válaszok listája (skálaértékek).
        /// </summary>
        public ImmutableArray<int> Answers = answers;

        /// <summary>
        /// Az értékek jelentése (pl. „1 = Egyáltalán nem értek egyet, 5 = Teljesen egyetértek”).
        /// </summary>
        public string ValueMeanings { get; init; } = valueMeanings;

        /// <summary>
        /// A skála minimum értéke (pl. 1).
        /// </summary>
        public int MinimumScale { get; init; } = minimumScale;

        /// <summary>
        /// A skála maximum értéke (pl. 5).
        /// </summary>
        public int MaximumScale { get; init; } = maximumScale;

        /// <summary>Átlag.</summary>
        public double MeanValue { get; private set; }

        /// <summary>Medián.</summary>
        public double MedianValue { get; private set; }

        /// <summary>Módusz.</summary>
        public double ModeValue { get; private set; }

        /// <summary>Szórás.</summary>
        public double StandardDeviation { get; private set; }

        /// <summary>Legnagyobb érték.</summary>
        public int MaximumRate { get; private set; }

        /// <summary>Legkisebb érték.</summary>
        public int MinimumRate { get; private set; }

        /// <summary>Egyetértési arány (%).</summary>
        public double AgreementRate { get; private set; }

        /// <summary>Elégedettségi index (0–100%).</summary>
        public double SatisfactionIndex { get; private set; }

        #endregion

        /// <summary>
        /// A nyers adatok feldolgozása és a statisztikai mutatók kiszámítása.
        /// <para>
        /// - Medián, átlag, módusz, szórás  
        /// - Minimum és maximum  
        /// - Egyetértési arány (küszöb: skála közepe)  
        /// - Elégedettségi index (átlag normalizálása 0–100%-ra)  
        /// </para>
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            MedianValue = CalculateMedianValue(Answers);
            MeanValue = CalculateMeanValue(Answers);
            ModeValue = CalculateModeValueRobust(Answers);
            StandardDeviation = CalculateStandardDeviation(Answers);
            MaximumRate = GetMaximumValue(Answers);
            MinimumRate = GetMinimumValue(Answers);
            AgreementRate = CalculateAgreementRate(Answers, (MaximumScale - MinimumScale) / 2);
            SatisfactionIndex = CalculateSatisfactionIndex(Answers, MinimumScale, MaximumScale);
            return this;
        }

        /// <summary>
        /// A hozzá tartozó riportkomponens előállítása (PDF-be illeszthető).
        /// </summary>
        public override IComponent CompileComponent()
        {
            return new LikertScaleReportComponent(this);
        }
    }
}
