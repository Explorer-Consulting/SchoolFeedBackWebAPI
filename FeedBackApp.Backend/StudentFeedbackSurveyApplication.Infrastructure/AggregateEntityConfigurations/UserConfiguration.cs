using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure.AggregateEntityConfigurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            throw new NotImplementedException();
        }
    }
}
