using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Statistical evaluation data model for single-choice questions.
    /// <para>
    /// Supports two types of questions:
    /// <list type="bullet">
    /// <item><see cref="SingleChoice.REGULAR"/> – selection from predefined options (with computed statistics).</item>
    /// <item><see cref="SingleChoice.CUSTOM"/> – mixed mode: numeric answers are used for statistics,
    /// while non-numeric answers are kept separately without statistical evaluation.</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class SingleChoiceEvaluationData(
        string questionStatement,
        ImmutableArray<string> questionOptions,
        SingleChoice type,
        ImmutableArray<int> questionOptionAnswers,
        ImmutableArray<string> questionOpenAnswers
    ) : EvaluationData
    {
        #region Question-specific properties

        /// <summary>The text of the question.</summary>
        public string QuestionStatement { get; init; } = questionStatement;

        /// <summary>List of predefined options.</summary>
        public ImmutableArray<string> QuestionOptions = questionOptions;

        /// <summary>
        /// Numeric representation of answers.  
        /// For <see cref="SingleChoice.REGULAR"/>: indices into <see cref="QuestionOptions"/>.  
        /// For <see cref="SingleChoice.CUSTOM"/>: raw numeric values (e.g., 1..5), not indices.
        /// </summary>
        public ImmutableArray<int> QuestionOptionAnswers = questionOptionAnswers;

        /// <summary>List of non-numeric (free text) answers.</summary>
        public ImmutableArray<string> QuestionOpenAnswers = questionOpenAnswers;

        /// <summary>The type of the question: <see cref="SingleChoice.REGULAR"/> or <see cref="SingleChoice.CUSTOM"/>.</summary>
        public SingleChoice Type = type;

        /// <summary>Mean value (for both REGULAR and CUSTOM numeric answers).</summary>
        public double MeanValue { get; private set; }

        /// <summary>Median value (for both REGULAR and CUSTOM numeric answers).</summary>
        public double MedianValue { get; private set; }

        /// <summary>Mode value (for both REGULAR and CUSTOM numeric answers).</summary>
        public double ModeValue { get; private set; }

        /// <summary>
        /// Absolute frequency.  
        /// REGULAR: aggregated by option names.  
        /// CUSTOM: aggregated by numeric values (e.g., "1", "2", "3"...).
        /// </summary>
        public Dictionary<string, int> Frequencies { get; private set; } = [];

        /// <summary>Relative frequencies in % (for REGULAR and CUSTOM numeric answers).</summary>
        public Dictionary<string, double> RelativeFrequencies { get; private set; } = [];

        #endregion

        /// <summary>
        /// Executes the statistical evaluation.
        /// <para>
        /// <b>REGULAR:</b> calculates mean, median, mode, and frequencies by option.  
        /// <b>CUSTOM:</b> if numeric answers exist, calculates mean, median, mode, and frequencies;
        /// non-numeric answers remain unchanged and are displayed separately in the report.
        /// </para>
        /// </summary>
        public override EvaluationData EvaluateData()
        {
            if (Type == SingleChoice.REGULAR)
            {
                if (QuestionOptionAnswers.Length == 0)
                    return this;

                MeanValue = CalculateMeanValue(QuestionOptionAnswers);
                MedianValue = CalculateMedianValue(QuestionOptionAnswers);
                ModeValue = CalculateModeValue(QuestionOptionAnswers);
                Frequencies = CalculateFrequency(QuestionOptions, QuestionOptionAnswers);
                RelativeFrequencies = CalculateRelativeFrequencyPercent(Frequencies, QuestionOptionAnswers.Length);
                return this;
            }

            // CUSTOM
            if (QuestionOptionAnswers.Length > 0)
            {
                // In CUSTOM mode, QuestionOptionAnswers contains raw numeric values (not option indices).
                MeanValue = CalculateMeanValue(QuestionOptionAnswers);
                MedianValue = CalculateMedianValue(QuestionOptionAnswers);
                ModeValue = CalculateModeValue(QuestionOptionAnswers);
                Frequencies = CalculateFrequency(QuestionOptions, QuestionOptionAnswers);
                RelativeFrequencies = CalculateRelativeFrequencyPercent(Frequencies, QuestionOptionAnswers.Length);
            }
            // If no numeric answers exist, statistical fields remain at default (0/empty),
            // and only QuestionOpenAnswers will be relevant in the report.

            return this;
        }

        /// <summary>
        /// Creates the corresponding report component (for PDF export).
        /// </summary>
        public override IComponent CompileComponent()
        {
            return new SingleChoiceReportComponent(this);
        }
    }
}
