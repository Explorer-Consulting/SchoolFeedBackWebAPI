using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure.AggregateEntityConfigurations
{
    public sealed class QuestionnaireTemplateConfiguration : IEntityTypeConfiguration<QuestionnaireTemplateDocument>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireTemplateDocument> builder)
        {
            throw new NotImplementedException();
        }
    }
}
