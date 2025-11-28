using Core.DomainModels;
using Infrastructure.ConfigurationAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.AggregateConfigurations
{
    [AggregateConfiguration(ContainerName: "QuestionnaireResponses", Description: "Stores assigned questionnaire results")]
    public sealed class QuestionnaireResponseStorageConfiguration(ILogger<QuestionnaireResponseStorageConfiguration> logger): IEntityTypeConfiguration<QuestionnaireResponse>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireResponse> builder)
        {
            logger.LogInformation("Configuring QuestionnaireResponse entity...");

            builder.ToContainer("QuestionnaireResponses");

            builder.HasKey(r => r.QuestionnaireResponseStorageID);
            builder.HasPartitionKey(r => r.QuestionnaireResponseStorageID);

            builder.Property(r => r.QuestionnaireResponseStorageID)
                   .ToJsonProperty("id")
                   .IsRequired();

            builder.Property(r => r.QuestionnaireResponseBusinessID)
                   .IsRequired();

            builder.Property(r => r.QuestionnaireTemplateBusinessID)
                   .IsRequired();

            builder.Property(r => r.AssigneeID)
                   .IsRequired();

            builder.Property(r => r.Status)
                   .IsRequired();

            builder.Property(r => r.Tags);

            builder.OwnsMany(r => r.QuestionResponses);

            logger.LogInformation("QuestionnaireResponse entity configured successfully.");
        }
    }
}
