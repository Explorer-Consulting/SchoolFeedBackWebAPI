using ApplicationLayer.Interfaces;
using Core.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjects
{
    public record QuestionnaireTemplateDTO() : IAggregateDTORoot
    {
        // Business ID for application logic
        public required string QuestionnaireTemplateBusinessID { get; init; }
        // Metadata about the questionnaire template
        public required QuestionnaireTemplateMetadataDTO Metadata { get; init; }
        // Collection of question items in the questionnaire template
        public required ICollection<QuestionItemDTO> QuestionItems { get; init; }
    }
}
