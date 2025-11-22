using Core.Interfaces;
using FeedBackApp.Core.DomainModels.AssignedQuestionnaireModels;
using NUlid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.AssignedQuestionnaireModels
{
    public sealed class AssignedQuestionnaireAttributeHeader
    {
        public ULID TrafficId { get; init; } = ULID.NewUlid();

        public ULID StorageId { get; init; } = ULID.NewUlid();

        public ULID QuestionnaireTemplateId { get; init; }

        public required ResponseStatus Status { get; set; }

        // the assigned user id
        public required ULID AssigneeId { get; init; }

        public required string EvaluationTarget { get; init; }
    }
}
