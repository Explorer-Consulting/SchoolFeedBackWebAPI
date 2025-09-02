using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class SingleChoiceEvaluationData(string questionStatement, Dictionary<string, int> questionOptions, SingleChoice type) : EvaluationData
    {
        #region Question-specific properties
        public required string QuestionStatement { get; init; } = questionStatement;
        public required Dictionary<string, int> QuestionOptions { get; init; } = questionOptions;
        public required SingleChoice Type { get; init; } = type;
        public required ImmutableArray<string>? Answers; 

        #endregion
        protected override void EvaluateData()
        {
            
        }
    }
}
