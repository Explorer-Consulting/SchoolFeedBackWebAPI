using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.Model.UserIdentityModels
{
    public class AuthenticationProvider
    {

        /// <summary>
        /// Gets or sets the unique identifier assigned to the user by the external authentication provider.
        /// </summary>
        /// <remarks>This value is typically provided by third-party identity services such as OAuth or
        /// OpenID Connect providers. It is used to associate the local user account with the corresponding external
        /// account.</remarks>
        public required string ExternalProviderUserId { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the user.
        /// </summary>
        public required string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the identity has been verified.
        /// </summary>
        public required bool IsVerifiedIdentity { get; set; }

        /// <summary>
        /// Gets or sets the name of the identity issuer associated with the entity.
        /// </summary>
        public required string IdentityIssuer { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the entity was linked.
        /// </summary>
        public required DateTimeOffset? LinkedAtTime { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the item was last used.
        /// </summary>
        public required DateTimeOffset? LastUsedAt { get; set; }
    }
}
