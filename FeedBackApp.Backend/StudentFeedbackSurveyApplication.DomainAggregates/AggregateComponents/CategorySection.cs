using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentFeedbackSurveyApplication.Domain.AggregateComponents
{
    /// <summary>
    /// Represents a section containing a category and its associated question template components.
    /// </summary>
    public sealed class CategorySection
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public required string CategoryName { get; set; }
        /// <summary>
        /// Gets or sets the collection of components that define the structure and content of the question template.
        /// </summary>

        public required IReadOnlyList<QuestionTemplateComponent> QuestionTemplateComponents { get; set; }
    }
}
