using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure.AggregateEntityConfigurations
{
    public sealed class QuestionnaireResponseConfiguration : IEntityTypeConfiguration<QuestionnaireResponseDocument>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireResponseDocument> builder)
        {
            throw new NotImplementedException();
        }
    }
}
