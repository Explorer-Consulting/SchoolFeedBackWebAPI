using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class LikertScaleEvaluationData(string questionStatement, List<int> answers, int minimumScale, int maximumScale) : EvaluationData
    {
        // Likert-scale question statement
        #region Question-specific properties
        public required string QuestionStatement { get; init; } = questionStatement;
        public required List<int> Answers { get; init; } = answers;
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

        protected override ReportComponent EvaluateData()
        {
            MedianValue = CalculateMedianValue(Answers);
            MeanValue = CalculateMeanValue(Answers);
            ModeValue = CalculateModeValue(Answers);
            StandardDeviation = CalculateStandardDeviation(Answers);
            MaximumRate = GetMaximumValue(Answers);
            MinimumRate = GetMinimumValue(Answers);
            AgreementRate = CalculateAgreementRate(Answers, (MaximumScale - MinimumScale) / 2);
            SatisfactionIndex = CalculateSatisfactionIndex(Answers, MinimumScale, MaximumScale);
            return new LikertScaleReportComponent(this);
        }
    }
}
