using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    /// <summary>
    /// Represents a dependency condition for a question, specifying which answers must be selected for the dependency
    /// to be triggered.
    /// </summary>
    /// <remarks>Use this class to define conditional logic between questions, such as showing or enabling a
    /// question only when specific answers are selected in another question. This type is typically used in survey or
    /// form workflows to manage dynamic question visibility or behavior.</remarks>
    public sealed class QuestionDependency : IAggregateProperty
    {
        /// <summary>
        /// Gets or sets the unique identifier of the question to which this dependency refers.
        /// </summary>
        public required string QuestionID { get; set; } = default!;// ID of the question this dependency refers to
        
        public required ICollection<string> ExpectedAnswerIndexes { get; set; } = default!; // Answers that trigger this
    }
}
