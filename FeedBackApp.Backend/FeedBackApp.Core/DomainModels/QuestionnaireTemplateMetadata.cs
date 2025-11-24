using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    public sealed class QuestionnaireTemplateMetadata() : IAggregateProperty
    {
        // Title of the questionnaire template
        public required string Title { get; set; }
        // Description of the questionnaire template
        public required string Description { get; set; }
        // Creation, activation, and expiration dates
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset ActivationDate { get; set; }
        public required DateTimeOffset ExpirationDate { get; set; }
        // Flag indicating if self-enrollment is allowed
        public required bool SelfEnrollmentAllowed { get; set; }
    }
}
