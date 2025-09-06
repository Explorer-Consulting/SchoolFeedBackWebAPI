using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class MultipleChoiceEvaluationData(
        string questionStatement,
        ImmutableArray<string> answerOptions,
        ImmutableArray<int> answers
    ) : EvaluationData
    {
        #region Inputs

        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<string> AnswerOptions { get; init; } = answerOptions;

        public ImmutableArray<int> Answers { get; init; } = answers;

        #endregion

        #region Outputs

        /// <summary>Abszolút gyakoriság opciónév szerint.</summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>Relatív gyakoriság százalékban opciónév szerint.</summary>
        public Dictionary<string, double> RelativeFrequenciesPercent { get; private set; } = [];

        /// <summary>Együtt-előfordulási mátrix (A,B) → hány alkalommal fordul elő a két opció.</summary>
        public Dictionary<(string A, string B), int> Cooccurrences { get; private set; } = [];

        #endregion

        public override EvaluationData EvaluateData()
        {
            // 1) Abszolút gyakoriság
            Frequencies = CalculateFrequency(AnswerOptions, Answers);

            // 2) Relatív gyakoriság (%)
            RelativeFrequenciesPercent = CalculateRelativeFrequencyPercent(Frequencies);

            return this;
        }
        public override IComponent CompileComponent()
        {
            return new MultipleChoiceReportComponent(this);
        }
    }
}
