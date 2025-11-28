using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels
{
    public sealed class User : IAggregateProperty
    {
        public required string UserStorageID { get; set; } = default!; // user ID for Cosmos
        public required string BusinessID { get; set; } = default!; // user ID for business logic
        public required string EmailAddress { get; set; } = default!; // user's email address
        public required EmailServiceProvider ServiceProvider { get; set; } = default!; // user's email service provider (e.g., Google, Microsoft)
    }
}
