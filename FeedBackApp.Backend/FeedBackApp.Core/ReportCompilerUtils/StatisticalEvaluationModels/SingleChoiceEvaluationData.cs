using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class SingleChoiceEvaluationData(string questionStatement, ImmutableArray<string> questionOptions, SingleChoice type, ImmutableArray<int> questionOptionAnswers, ImmutableArray<string> questionOpenAnswers) : EvaluationData
    {
        #region Question-specific properties
        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<string> QuestionOptions = questionOptions;
        public ImmutableArray<int> QuestionOptionAnswers = questionOptionAnswers;
        public ImmutableArray<string> QuestionOpenAnswers = questionOpenAnswers;
        public SingleChoice Type = type;
        public double MeanValue { get; private set; }
        public double MedianValue { get; private set; }
        public double ModeValue { get; private set; }

        public Dictionary<string, int> Frequencies { get; private set; } = [];
        public Dictionary<string, double> RelativeFrequencies { get; private set; } = [];
        #endregion
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
        public override IComponent CompileComponent()
        {
            return new SingleChoiceReportComponent(this);
        }
    }
}
