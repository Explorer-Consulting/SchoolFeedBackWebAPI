using Core.DomainModels.QuestionnaireTemplateModels;
using Infrastructure.ConfigurationAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.AggregateConfigurations
{
    [AggregateConfiguration(ContainerName: "QuestionnaireTemplates", Description: "Stores questionnaire templates")]
    public sealed class QuestionnaireTemplateStorageConfiguration() : IEntityTypeConfiguration<QuestionnaireTemplate>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireTemplate> builder)
        {
            builder.ToContainer("QuestionnaireTemplateContainer");
            builder.HasPartitionKey(key => key.Header.StorageId.ToString());
            builder.HasNoDiscriminator();
        }
    }
}
