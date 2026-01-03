using FeedBackApp.Core.Model.UserIdentityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUlid;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var ulidToString = new ValueConverter<Ulid, string>(
                v => v.ToString(),
                v => Ulid.Parse(v));

            builder.ToContainer("Users");

            builder.HasKey(x => x.UserId);
            builder.HasPartitionKey(x => x.UserId);

            builder.Property(x => x.UserId)
                .ToJsonProperty("userId")
                .HasConversion(ulidToString)
                .IsRequired();

            builder.Property(x => x.IsActiveUser)
                .ToJsonProperty("isActiveUser")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .ToJsonProperty("createdAt")
                .IsRequired();

            builder.Property(x => x.LastLoginAt)
                .ToJsonProperty("lastLoginAt")
                .IsRequired();

            builder.Property(x => x.Role)
                .ToJsonProperty("role")
                .HasConversion<string>()
                .IsRequired();

            builder.OwnsMany(x => x.IdentityProviders, nav =>
            {
                nav.ToJsonProperty("identityProviders");

                nav.Property(x => x.ExternalProviderUserId)
                    .ToJsonProperty("externalProviderUserId")
                    .IsRequired();

                nav.Property(x => x.EmailAddress)
                    .ToJsonProperty("emailAddress")
                    .IsRequired();

                nav.Property(x => x.IsVerifiedIdentity)
                    .ToJsonProperty("isVerifiedIdentity")
                    .IsRequired();

                nav.Property(x => x.IdentityIssuer)
                    .ToJsonProperty("identityIssuer")
                    .IsRequired();

                nav.Property(x => x.LinkedAtTime)
                    .ToJsonProperty("linkedAtTime")
                    .IsRequired(false);

                nav.Property(x => x.LastUsedAt)
                    .ToJsonProperty("lastUsedAt")
                    .IsRequired(false);
            });
        }
    }
}
