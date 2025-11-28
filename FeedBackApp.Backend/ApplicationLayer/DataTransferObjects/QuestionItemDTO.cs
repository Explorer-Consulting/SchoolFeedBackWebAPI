using Core.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjects
{
    public record QuestionItemDTO
    {
        // Unique identifier for the question item
        public required string QuestionID { get; init; }
        // Type of the question (e.g., multiple choice, open-ended)
        public required QuestionItemType QuestionType { get; init; }
        // The actual question statement presented to users
        public required string QuestionStatement { get; init; }
        // Collection of possible answer options for the question
        public required ICollection<string> AnswerOptions { get; init; }
        // Collection of dependencies that determine the visibility or relevance of the question
        public required ICollection<QuestionDependencyDTO> Dependencies { get; init; }
    }
}
