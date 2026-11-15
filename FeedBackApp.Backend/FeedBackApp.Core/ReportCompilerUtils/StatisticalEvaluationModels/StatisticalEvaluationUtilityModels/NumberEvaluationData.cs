using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class NumberEvaluationData(string questionId, string questionStatement, ImmutableArray<double> answers)
        : EvaluationData
    {
        public string QuestionId { get; init; } = questionId;
        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<double> Answers { get; init; } = answers;

        public double Mean { get; private set; }
        public double Median { get; private set; }
        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
        public double StandardDeviation { get; private set; }

        public override EvaluationData EvaluateData()
        {
            var sorted = Answers.Sort();
            int n = sorted.Length;

            Mean = sorted.Average();
            Median = n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
            Minimum = sorted[0];
            Maximum = sorted[^1];
            StandardDeviation = Math.Sqrt(sorted.Sum(x => (x - Mean) * (x - Mean)) / n); // populációs, mint a Likertnél
            return this;
        }

        public override IComponent CompileComponent() => new NumberReportComponent(this);
    }
}