using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels
{

    public sealed class QuestionnaireTemplate : IAggregateRoot
    {
        // Storage ID for Cosmos DB
        public required string QuestionnaireTemplateStorageID { get; set; } = default!;
        // Business ID for application logic
        public required string QuestionnaireTemplateBusinessID { get; set; } = default!;
        // Metadata about the questionnaire template
        public required QuestionnaireTemplateMetadata Metadata { get; set; } = default!;
        // Collection of question items in the questionnaire template
        public required ICollection<QuestionItem> QuestionItems { get; set; } = default!;
    }
}
