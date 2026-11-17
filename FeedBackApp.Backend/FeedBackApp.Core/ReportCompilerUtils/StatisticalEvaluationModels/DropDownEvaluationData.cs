using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class DropDownEvaluationData : EvaluationData
    {
        public string QuestionStatement { get; }

        public ImmutableArray<DropDownItem> Items { get; }

        public ImmutableArray<string> Answers { get; }

        public Dictionary<string, int> Frequencies { get; private set; }
            = new(StringComparer.Ordinal);

        public Dictionary<string, double> RelativeFrequencies
        { get; private set; } = new(StringComparer.Ordinal);

        public int ValidAnswerCount { get; private set; }

        public int UnknownAnswerCount { get; private set; }

        public DropDownEvaluationData(
            string questionId,
            string questionStatement,
            ImmutableArray<DropDownItem> items,
            ImmutableArray<string> answers)
        {
            QuestionId = questionId;
            QuestionStatement = questionStatement;
            Items = items.IsDefault ? [] : items;
            Answers = answers.IsDefault ? [] : answers;
        }

        public override EvaluationData EvaluateData()
        {
            Frequencies = Items.ToDictionary(
                item => item.Id,
                _ => 0,
                StringComparer.Ordinal);

            ValidAnswerCount = 0;
            UnknownAnswerCount = 0;

            foreach (var answer in Answers)
            {
                if (Frequencies.TryGetValue(answer, out int count))
                {
                    Frequencies[answer] = count + 1;
                    ValidAnswerCount++;
                }
                else
                {
                    UnknownAnswerCount++;
                }
            }

            RelativeFrequencies =
                CalculateRelativeFrequencyPercent(Frequencies);

            return this;
        }

        public override IComponent CompileComponent() =>
            new DropDownReportComponent(this);
    }
}