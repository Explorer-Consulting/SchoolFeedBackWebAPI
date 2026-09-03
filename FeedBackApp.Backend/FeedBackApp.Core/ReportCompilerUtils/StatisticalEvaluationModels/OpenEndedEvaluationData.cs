using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Statistical evaluation data model for open-ended (text) questions.
    /// <para>
    /// Stores the question text and the received free-text answers.  
    /// Does not perform numerical statistical evaluation — the <see cref="EvaluateData"/> 
    /// method simply returns the current instance.
    /// </para>
    /// </summary>
    public sealed class OpenEndedEvaluationData(string questionId, string questionStatement, ImmutableArray<string> answers)
        : EvaluationData
    {
        #region Inputs
        public string QuestionId { get; init; } = questionId;

        /// <summary>
        /// The question text.
        /// </summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>
        /// The list of received free-text answers.
        /// <para>
        /// It is recommended to filter out empty or whitespace-only strings during data loading.
        /// </para>
        /// </summary>
        public ImmutableArray<string> Answers { get; init; } = answers;
        #endregion

        /// <summary>
        /// No statistical indicators are calculated; simply returns the current instance.
        /// </summary>
        public override EvaluationData EvaluateData() => this;

        /// <summary>
        /// Creates the corresponding report component for open-ended questions.
        /// </summary>
        public override IComponent CompileComponent() => new OpenEndedReportComponent(this);
    }
}
