using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainAggregates.AggregateRoots
{
    public sealed class QuestionnaireResponse
    {
        public required ULID BusinessID { get; set; }
        public required string SurrogateID { get; set; }
        public required string QuestionnaireTemplateSurrogateID { get; set; }
        public required string AssigneeID { get; set; }

    }
}
