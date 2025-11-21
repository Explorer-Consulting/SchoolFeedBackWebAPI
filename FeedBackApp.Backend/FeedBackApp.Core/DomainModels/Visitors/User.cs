using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainModels.Visitors
{
    public sealed class User
    {
        public ULID TrafficId { get; init; } = ULID.NewUlid();
        public ULID VisitorId { get; init; } = ULID.NewUlid();
        // something that refers to the authentication service
        public required string Email { get; init; }
    }
}
