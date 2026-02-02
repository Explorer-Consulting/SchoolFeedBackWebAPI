using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentFeedbackSurveyApplication.Domain.AggregateComponents
{
    /// <summary>
    /// Represents a dependency for a question template, specifying the order and allowed values required for
    /// evaluation.
    /// </summary>
    public sealed class QuestionTemplateDependency
    {
        /// <summary>
        /// Gets the order number that determines this object's position in dependency resolution sequences.
        /// </summary>
        /// <remarks>Objects with lower dependency order numbers are resolved before those with higher
        /// numbers. This property is typically used to control initialization or processing order when dependencies
        /// exist between objects.</remarks>
        public required int DependencyOrderNumber { get; init; }
        /// <summary>
        /// Gets the list of allowed integer values for this instance.
        /// </summary>
        public required IReadOnlyList<int> AllowedValues { get; init; } = [];
    }
}
