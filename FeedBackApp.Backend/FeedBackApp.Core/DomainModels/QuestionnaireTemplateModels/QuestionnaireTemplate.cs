using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainModels.QuestionnaireTemplateModels
{
    public sealed class QuestionnaireTemplate
    {
        public required QuestionnaireTemplateAttributeHeader Header { get; init; }

    }
}
