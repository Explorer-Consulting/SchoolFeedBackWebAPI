using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    public sealed class QuestionDependency : IAggregateProperty
    {
        public required string QuestionID { get; set; } // ID of the question this dependency refers to
        public required ICollection<string> ExpectedAnswerIndexes { get; set; } // Answers that trigger this
    }
}
