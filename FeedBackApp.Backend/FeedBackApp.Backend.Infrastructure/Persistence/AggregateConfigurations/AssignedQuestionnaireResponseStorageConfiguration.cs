using Core.DomainModels.AssignedQuestionnaireModels;
using Infrastructure.ConfigurationAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.AggregateConfigurations
{
    [AggregateConfiguration(ContainerName: "QuestionnaireResponses", Description: "Stores assigned questionnaire results")]
    public sealed class AssignedQuestionnaireResponseStorageConfiguration : IEntityTypeConfiguration<AssignedQuestionnaireResponse>
    {
        public void Configure(EntityTypeBuilder<AssignedQuestionnaireResponse> builder)
        {
            throw new NotImplementedException();
        }
    }
}
