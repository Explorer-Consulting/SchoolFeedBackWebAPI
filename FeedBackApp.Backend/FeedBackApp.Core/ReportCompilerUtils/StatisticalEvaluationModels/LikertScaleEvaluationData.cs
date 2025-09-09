using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Statistical evaluation data model for Likert-scale questions.
    /// <para>
    /// Contains the raw responses, the scale parameters, and the calculated indicators 
    /// (mean, median, mode, standard deviation, min/max, satisfaction index, agreement rate).
    /// </para>
    /// After calling <see cref="EvaluateData"/>, all computed values are populated.  
    /// The <see cref="CompileComponent"/> method returns the corresponding QuestPDF component.
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
        /// The question text (e.g., "The teacher explained clearly").
        /// </summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>
        /// The list of received responses (scale values).
        /// </summary>
        public ImmutableArray<int> Answers = answers;

        /// <summary>
        /// The meaning of the values (e.g., "1 = Strongly disagree, 5 = Strongly agree").
        /// </summary>
        public string ValueMeanings { get; init; } = valueMeanings;

        /// <summary>
        /// The minimum scale value (e.g., 1).
        /// </summary>
        public int MinimumScale { get; init; } = minimumScale;

        /// <summary>
        /// The maximum scale value (e.g., 5).
        /// </summary>
        public int MaximumScale { get; init; } = maximumScale;

        /// <summary>Mean.</summary>
        public double MeanValue { get; private set; }

        /// <summary>Median.</summary>
        public double MedianValue { get; private set; }

        /// <summary>Mode.</summary>
        public double ModeValue { get; private set; }

        /// <summary>Standard deviation.</summary>
        public double StandardDeviation { get; private set; }

        /// <summary>Maximum observed value.</summary>
        public int MaximumRate { get; private set; }

        /// <summary>Minimum observed value.</summary>
        public int MinimumRate { get; private set; }

        /// <summary>Agreement rate (%).</summary>
        public double AgreementRate { get; private set; }

        /// <summary>Satisfaction index (0–100%).</summary>
        public double SatisfactionIndex { get; private set; }

        #endregion

        /// <summary>
        /// Processes the raw data and calculates all statistical indicators.
        /// <para>
        /// - Median, mean, mode, standard deviation  
        /// - Minimum and maximum  
        /// - Agreement rate (threshold = midpoint of the scale)  
        /// - Satisfaction index (mean normalized to 0–100%)  
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
        /// Creates the corresponding report component (for embedding into a PDF).
        /// </summary>
        public override IComponent CompileComponent()
        {
            return new LikertScaleReportComponent(this);
        }
    }
}
