using NUlid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots
{
    public sealed class QuestionnaireResponseDocument : AggregateEntity
    {
        public required ULID QuestionnaireTemplateId { get; set; }
        public required ULID PartitionKey { get; set; }
    }
}
