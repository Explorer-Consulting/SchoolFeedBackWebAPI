using Core.DomainModels;
using Infrastructure.ConfigurationAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.AggregateConfigurations
{
    [AggregateConfiguration(ContainerName: "QuestionnaireTemplates", Description: "Stores questionnaire templates")]
    public sealed class QuestionnaireTemplateStorageConfiguration(ILogger<QuestionnaireTemplateStorageConfiguration> logger): IEntityTypeConfiguration<QuestionnaireTemplate>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireTemplate> builder)
        {
            logger.LogInformation("Configuring entity: {EntityName}", nameof(QuestionnaireTemplate));

            builder.ToContainer("QuestionnaireTemplates");

            builder.HasKey(t => t.QuestionnaireTemplateStorageID);
            builder.HasPartitionKey(t => t.QuestionnaireTemplateStorageID);

            builder.Property(t => t.QuestionnaireTemplateStorageID)
                   .ToJsonProperty("id")
                   .IsRequired();

            builder.Property(t => t.QuestionnaireTemplateBusinessID)
                   .IsRequired();

            builder.ComplexProperty(t => t.Metadata);

            builder.OwnsMany(t => t.QuestionItems);

            logger.LogInformation("Finished configuring entity: {EntityName}", nameof(QuestionnaireTemplate));
        }
    }
}
