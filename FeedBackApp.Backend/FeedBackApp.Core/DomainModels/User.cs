using Core.DomainModels.DomainEnums;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.Users
{
    public sealed class User() : IAggregateProperty
    {
        public required UserStorageID StorageID { get; set; } // user ID for Cosmos
        public required UserBusinessID BusinessID { get; set; } // user ID for business logic
        public required string EmailAddress { get; set; } // user's email address
        public required EmailServiceProvider ServiceProvider { get; set; } // user's email service provider (e.g., Google, Microsoft)
    }
}
