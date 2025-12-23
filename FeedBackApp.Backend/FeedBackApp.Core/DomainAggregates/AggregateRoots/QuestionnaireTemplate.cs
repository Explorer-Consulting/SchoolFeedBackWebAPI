using FeedBackApp.Core.DomainAggregates.AggregateComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainAggregates.AggregateRoots
{
    public sealed class QuestionnaireTemplate
    {
        public required ULID BusinessID { get; set; }
        public required string SurrogateID { get; set; }
        public required string Title { get; set; }
        public required bool SelfEnrollmentAllowed { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset ActivationDate { get; set; }
        public required DateTimeOffset ExpirationDate { get; set; }
        public required ICollection<QuestionItem> QuestionItems { get; set; }
    }
}
