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
        public string Title { get; set; } = default!;
        // Description of the questionnaire template
        public string Description { get; set; } = default!;
        // Creation, activation, and expiration dates
        public DateTimeOffset CreatedAt { get; set; } = default!;
        public DateTimeOffset ActivationDate { get; set; } = default!;
        public DateTimeOffset ExpirationDate { get; set; } = default!;
        // Flag indicating if self-enrollment is allowed
        public bool SelfEnrollmentAllowed { get; set; } = default!;
    }
}
