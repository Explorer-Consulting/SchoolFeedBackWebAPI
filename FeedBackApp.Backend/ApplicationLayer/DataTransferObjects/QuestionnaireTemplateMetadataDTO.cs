using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjects
{
    public record QuestionnaireTemplateMetadataDTO
    {
        public required string Title { get; init; }
        // Description of the questionnaire template
        public required string Description { get; init;}
        // Creation, activation, and expiration dates
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset ActivationDate { get; init; }
        public required DateTimeOffset ExpirationDate { get; init; }
        // Flag indicating if self-enrollment is allowed
        public required bool SelfEnrollmentAllowed { get; init; }
    }
}
