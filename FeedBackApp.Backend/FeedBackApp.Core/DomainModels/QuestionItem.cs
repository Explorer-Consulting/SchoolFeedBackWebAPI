using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    public sealed class QuestionItem : IAggregateProperty
    {
        // Unique identifier for the question item
        public required string QuestionID { get; set; }
        // Type of the question (e.g., multiple choice, open-ended)
        public required QuestionItemType QuestionType { get; set; }
        // The actual question statement presented to users
        public required string QuestionStatement { get; set; }
        // Collection of possible answer options for the question
        public required ICollection<string> AnswerOptions { get; set; }
        // Collection of dependencies that determine the visibility or relevance of the question
        public required ICollection<QuestionDependency> Dependencies { get; set; }
    }
}
