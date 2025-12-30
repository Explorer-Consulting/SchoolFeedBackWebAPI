using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;

public sealed class EmailsToSendConfiguration : IEntityTypeConfiguration<EmailsToSend>
{
    public required string ContainerName { get; init; }

    public void Configure(EntityTypeBuilder<EmailsToSend> builder)
    {
        builder.ToContainer(ContainerName);

        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasDiscriminator<string>("DocumentType")
            .HasValue("EmailsToSend");

        builder.OwnsMany(x => x.EmailsToSendList, e =>
        {
            e.ToJsonProperty("emailsToSendList");

            e.Property(p => p.SurveyId).ToJsonProperty("surveyId").IsRequired();
            e.Property(p => p.SurveyName).ToJsonProperty("surveyName").IsRequired();
            e.Property(p => p.StartDate).ToJsonProperty("startDate");
            e.Property(p => p.EndDate).ToJsonProperty("endDate");
            e.Property(p => p.Emails).ToJsonProperty("emails");
            e.Property(p => p.Role).ToJsonProperty("role");
        });
    }
}
