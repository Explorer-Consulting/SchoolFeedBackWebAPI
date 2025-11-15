using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainModels.Visitors
{
    public sealed class Visitor
    {
        public ULID TrafficId { get; init; } = ULID.NewUlid();
        public ULID VisitorId { get; init; } = ULID.NewUlid();
        public required string Email { get; init; }
    }
}
