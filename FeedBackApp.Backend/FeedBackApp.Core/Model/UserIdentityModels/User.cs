using NUlid;

namespace FeedBackApp.Core.Model.UserIdentityModels
{
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user. (Cosmos-only)
        /// </summary>
        public required Ulid UserId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the user is currently active within the system.
        /// </summary>
        public required bool IsActiveUser { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        public required DateTimeOffset CreatedAt { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the user last successfully logged in.
        /// </summary>
        public required DateTimeOffset LastLoginAt { get; set; }
        /// <summary>
        /// Gets or sets the collection of identity providers used for authentication.
        /// </summary>
        /// <remarks>Each provider in the collection represents an external or internal authentication
        /// source that can be used to validate user identities. The collection must be populated with at least one
        /// provider before authentication operations can succeed.</remarks>
        public required ICollection<AuthenticationProvider> IdentityProviders {get; set; }
        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        public required UserRole Role { get; set; }

    }
}
