using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class LikertScaleEvaluationData(string questionStatement, ImmutableArray<int> answers, string valueMeanings, int minimumScale, int maximumScale) : EvaluationData
    {
        // Likert-scale question statement
        #region Question-specific properties
        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<int> Answers = answers;
        //az ertekek jelentesei
        public string ValueMeanings { get; init; } = valueMeanings;
        public int MinimumScale { get; init; } = minimumScale;
        public int MaximumScale { get; init; } = maximumScale;
        public double MeanValue { get; private set; }
        public double MedianValue { get; private set; }
        public double ModeValue { get; private set; }
        public double StandardDeviation { get; private set; }
        public int MaximumRate { get; private set; }
        public int MinimumRate { get; private set; }
        public double AgreementRate { get; private set; }
        public double SatisfactionIndex { get; private set; }
        #endregion

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
        public override IComponent CompileComponent()
        {
            return new LikertScaleReportComponent(this);
        }
    }
}
