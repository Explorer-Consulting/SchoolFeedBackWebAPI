using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;

public sealed class StudentWhiteListConfiguration : IEntityTypeConfiguration<StudentWhitelist>
{
    public required string ContainerName { get; init; }

    public void Configure(EntityTypeBuilder<StudentWhitelist> builder)
    {
        builder.ToContainer(ContainerName);

        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasDiscriminator<string>("DocumentType")
            .HasValue("StudentWhitelist");

        builder.Property(x => x.StudentEmails)
            .ToJsonProperty("studentEmails");
    }
}
