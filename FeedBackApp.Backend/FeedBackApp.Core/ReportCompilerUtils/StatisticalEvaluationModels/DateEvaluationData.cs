using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class DateEvaluationData(string questionId, string questionStatement, ImmutableArray<DateOnly> answers)
        : EvaluationData
    {
        public string QuestionId { get; init; } = questionId;
        public string QuestionStatement { get; init; } = questionStatement;
        public ImmutableArray<DateOnly> Answers { get; init; } = answers;

        public DateOnly? Earliest { get; private set; }
        public DateOnly? Latest { get; private set; }
        public IReadOnlyList<KeyValuePair<DateOnly, int>> Frequencies { get; private set; } = [];

        public override EvaluationData EvaluateData()
        {
            Earliest = Answers.Min();
            Latest = Answers.Max();
            Frequencies = Answers
                .GroupBy(d => d)
                .OrderBy(g => g.Key)
                .Select(g => new KeyValuePair<DateOnly, int>(g.Key, g.Count()))
                .ToList();
            return this;
        }

        public override IComponent CompileComponent() => new DateReportComponent(this);
    }
}