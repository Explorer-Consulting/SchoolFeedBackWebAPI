using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class OpenEndedEvaluationData(string questionStatement, ImmutableArray<string> answers)
        : EvaluationData
    {
        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<string> Answers { get; init; } = answers;

        public override EvaluationData EvaluateData() => this;

        public override IComponent CompileComponent() => new OpenEndedReportComponent(this);
    }
}
