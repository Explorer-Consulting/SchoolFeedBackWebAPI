using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public sealed class OpenEndedEvaluationData(string questionStatement, ImmutableArray<string> answers) : EvaluationData
    {
        public required string QuestionStatement { get; init; } = questionStatement;
        public required ImmutableArray<string> Answers { get; init; } = answers;

        protected override void EvaluateData()
        {
            /*
             ide majd johet valamilyen szures a csunya szavak miatt, de egyenlore itt semmi.
             */
        }
    }
}
