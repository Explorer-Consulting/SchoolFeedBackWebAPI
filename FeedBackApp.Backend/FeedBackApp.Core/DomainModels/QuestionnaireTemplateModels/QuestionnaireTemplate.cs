using Core.Interfaces;
using FeedBackApp.Core.DomainModels.QuestionnaireTemplateModels;
using FeedBackApp.Core.DomainModels.QuestionTemplateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.QuestionnaireTemplateModels
{
    public sealed class QuestionnaireTemplate : IAggregateRoot
    {
        public required QuestionnaireTemplateAttributeHeader Header { get; init; }
        public required IReadOnlyCollection<QuestionTemplate> Questions { get; init; }
    }
}
