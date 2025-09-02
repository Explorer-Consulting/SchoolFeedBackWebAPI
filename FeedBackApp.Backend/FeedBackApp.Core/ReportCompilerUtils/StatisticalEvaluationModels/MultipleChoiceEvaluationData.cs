using System.Collections.Immutable;
namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class MultipleChoiceEvaluationData(string questionStatement, ImmutableArray<string> questionOptions) : EvaluationData
    {
        #region Question-specific properties
        public required string QuestionStatement { get; init; } = questionStatement;
        public required ImmutableArray<string> QuestionOptions { get; init; } = questionOptions;
        public ImmutableArray<int> OptionFrequencies { get; private set; }
        public ImmutableArray<((int a, int b) pair, int count)> Cooccurances { get; private set; }

        #endregion
        protected override void EvaluateData()
        {
            throw new NotImplementedException();
        }
    }
}
