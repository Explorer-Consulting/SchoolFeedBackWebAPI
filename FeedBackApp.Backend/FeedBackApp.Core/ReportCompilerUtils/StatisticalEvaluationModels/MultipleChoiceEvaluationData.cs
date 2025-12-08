using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Statistical evaluation data model for Multiple Choice questions.
    /// <para>
    /// Takes the list of answer options and the received responses (option indexes) as input,
    /// and produces absolute and relative frequencies as output.  
    /// Optionally, it can also store co-occurrence counts.
    /// </para>
    /// </summary>
    public sealed class MultipleChoiceEvaluationData(
        string questionId,
        string questionStatement,
        ImmutableArray<string> answerOptions,
        ImmutableArray<ImmutableArray<int>> answers
    ) : EvaluationData
    {
        #region Inputs

        public string QuestionId { get; init; } = questionId;

        /// <summary>
        /// The text of the question.
        /// </summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>
        /// The list of available answer options (indices in <see cref="Answers"/> refer to these).
        /// </summary>
        public ImmutableArray<string> AnswerOptions { get; init; } = answerOptions;

        /// <summary>
        /// The received responses as option indices.
        /// <para>
        /// Each element represents the index of a selected option in the <see cref="AnswerOptions"/> array.
        /// </para>
        /// </summary>
        public ImmutableArray<ImmutableArray<int>> Answers { get; init; } = answers;

        #endregion

        #region Outputs

        /// <summary>
        /// Absolute frequencies by option name.
        /// </summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>
        /// Relative frequencies (percentage) by option name.
        /// </summary>
        public Dictionary<string, double> RelativeFrequenciesPercent { get; private set; } = [];

        /// <summary>
        /// Co-occurrence counts: (A, B) → number of times they appeared together.
        /// <para>
        /// Note: proper co-occurrence requires per-respondent <b>sets of answers</b>.  
        /// If only a flat index list is available, true co-occurrence cannot be computed.
        /// </para>
        /// </summary>
        public Dictionary<(string A, string B), int> Cooccurrences { get; private set; } = [];

        #endregion

        /// <summary>
        /// Runs the evaluation: calculates absolute and relative frequencies.
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            // Flatten the 2D array to calculate frequencies
            var flattenedAnswers = Answers.SelectMany(respondent => respondent).ToImmutableArray();

            // 1) Absolute frequency (option name → count)
            Frequencies = CalculateFrequency(AnswerOptions, flattenedAnswers);

            // 2) Relative frequency (%) – based on total count
            RelativeFrequenciesPercent = CalculateRelativeFrequencyPercent(Frequencies);

            // 3) Co-occurrences:
            // NOTE: This would require per-respondent sets of answers (e.g., IEnumerable<int[]>).
            // With the current flat index list, this information is not available,
            // so no co-occurrence calculation is performed here.
            // Cooccurrences = ...

            return this;
        }

        /// <summary>
        /// Creates the corresponding report component (for embedding into a PDF).
        /// </summary>
        public override IComponent CompileComponent()
        {
            return new MultipleChoiceReportComponent(this);
        }
    }
}
