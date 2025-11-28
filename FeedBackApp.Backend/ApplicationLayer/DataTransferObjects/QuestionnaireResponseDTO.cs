using ApplicationLayer.Interfaces;
using Core.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjects
{
    public record QuestionnaireResponseDTO : IAggregateDTORoot
    {
        public required string QuestionnaireResponseBusinessID { get; init; } // Business ID for application
        public required string QuestionnaireTemplateBusinessID { get; init; }// Associated template ID
        public required string AssigneeID { get; init; }// ID of the user who submitted the response
        public required ICollection<string> Tags { get; init; } // Tags for categorization
        public required ICollection<QuestionResponseDTO> QuestionResponses { get; init; } // User responses
        public required ResponseStatus Status { get; init; }// Current status of the response
    }
}
